/*
 * Copyright 2015-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "chip.h"
#include "timer.h"

/**
 * @c false when the timer is stopped or when the RTC_IRQn interrupt wasn't fired after being started (again).
 * @c true when the timer was started (again) and the interrupt was fired.
 */
static volatile bool sMeasurementTimeoutInterruptFired = false;

/**
 * @c false when the timer is stopped or when the CT16B0_IRQn interrupt wasn't fired after being started (again).
 * @c true when the timer was started (again) and the interrupt was fired.
 */
static volatile bool sHostTimeoutInterruptFired = false;

/* -------------------------------------------------------------------------------- */

void RTC_IRQHandler(void)
{
    RTC_INT_T status = Chip_RTC_Int_GetRawStatus(NSS_RTC);
    Chip_RTC_Int_ClearRawStatus(NSS_RTC, status);

    if (status & RTC_INT_WAKEUP) {
        /* The down counter of the RTC does not automatically reload. In this handler, only a flag is stored, that the
         * main thread has to check and act upon.
         * It is the responsibility of the main application to reload the RTC down counter. To ensure measurements are
         * taken as uniformly spread in time as possible, this is done immediately after checking the flag.
         * - Flag is checked
         * - If set, DoPeriodicMeasurements is called
         * - Inside that function, the RTC is reloaded with the correct seconds interval.
         * Reloading cannot be done here under interrupt, since the IC might wake up from Deep Power Down due to an
         * expired RTC, when this interrupt handler is not called.
         * When the main thread fails to check the flag sMeasurementTimeoutInterruptFired before going to Deep Power
         * Down, he will not reload it and thus not wake up anymore due to RTC. To prevent this, we unconditionally
         * program a new short timeout now, to ensure the RTC will only be stopped when Timer_StopMeasurementTimeout is
         * explicitly called: the next correct time to make a next measurement will be set in DoPeriodicMeasurements
         * anyway, overruling this.
         */
        Chip_RTC_Wakeup_SetReload(NSS_RTC, 1); /* Any small value will do. */
        sMeasurementTimeoutInterruptFired = true;
    }
}

void CT16B0_IRQHandler(void)
{
    if (Chip_TIMER_MatchPending(NSS_TIMER16_0, 0)) {
        Chip_TIMER_ClearMatch(NSS_TIMER16_0, 0);
        Timer_StopHostTimeout();
        sHostTimeoutInterruptFired = true;
    }
}

/* -------------------------------------------------------------------------------- */

void Timer_Init(void)
{
    /* RTC timer: down counter may be already running or have been expired just now. Ensure interrupts arrive. */
    Chip_RTC_Int_SetEnabledMask(NSS_RTC, RTC_INT_WAKEUP);
    NVIC_EnableIRQ(RTC_IRQn);

    /* 16-bit timer: nothing to do. Started on request only. */
    /* 32-bit timer: nothing to do. Started on request only. */
}

/* -------------------------------------------------------------------------------- */

void Timer_StartMeasurementTimeout(int seconds)
{
    Chip_RTC_Wakeup_SetControl(NSS_RTC, RTC_WAKEUPCTRL_DISABLE);

    sMeasurementTimeoutInterruptFired = false;
    if (seconds <= 0) {
        seconds = 1; /* Have it fired almost immediately. */
    }

    Chip_RTC_Wakeup_SetControl(NSS_RTC, RTC_WAKEUPCTRL_ENABLE | RTC_WAKEUPCTRL_AUTO);
    Chip_RTC_Wakeup_SetReload(NSS_RTC, seconds);
    Chip_SysCon_StartLogic_SetEnabledMask(SYSCON_STARTSOURCE_RTC);
    Chip_SysCon_StartLogic_ClearStatus(SYSCON_STARTSOURCE_RTC);
    NVIC_EnableIRQ(RTC_IRQn);
    Chip_RTC_Int_SetEnabledMask(NSS_RTC, RTC_INT_WAKEUP);
}

void Timer_StopMeasurementTimeout(void)
{
    Chip_RTC_Wakeup_SetControl(NSS_RTC, RTC_WAKEUPCTRL_DISABLE);
    NVIC_DisableIRQ(RTC_IRQn);
    Chip_RTC_Int_SetEnabledMask(NSS_RTC, RTC_INT_NONE);
    sMeasurementTimeoutInterruptFired = false;
}

bool Timer_CheckMeasurementTimeout(void)
{
    return sMeasurementTimeoutInterruptFired;
}

/* -------------------------------------------------------------------------------- */

void Timer_StartHostTimeout(int seconds)
{
    Chip_TIMER16_0_Init();
    Timer_StopHostTimeout();

    uint32_t matchval;
    int clockFreq = Chip_Clock_System_GetClockFreq();
    if (seconds >= 1) {
        matchval = (uint32_t)(seconds * clockFreq) >> 13; /* 13: see comment below */
    }
    else {
        /* Have it fired almost immediately: minimally 16 msec (@500kHz and above), maximally (@62.5kHz) 143 msec */
        matchval = ((uint32_t)(1 * clockFreq) >> (13 + 6)) + 1;
    }

    /* - The timer frequency is derived from the system clock frequency.
     * - A prescaler value of n will result in a divider of (n+1)
     * Set the timer to tick very slow - accuracy is not a concern here.
     * A large prescaler value that still gives a couple of units at the slowest system clock frequency is chosen:
     * 2**13 - 1. This gives 7 ticks per second @62.5kHz, 61 ticks @500kHz, 244 @2MHz, 976 @8MHz.
     */
    Chip_TIMER_PrescaleSet(NSS_TIMER16_0, (1<<13) - 1);

    Chip_TIMER_SetMatch(NSS_TIMER16_0, 0, matchval);
    Chip_TIMER_MatchEnableInt(NSS_TIMER16_0, 0);
    NVIC_EnableIRQ(CT16B0_IRQn); /* Interrupts are handled in CT16B0_IRQHandler() above. */
    Chip_TIMER_Enable(NSS_TIMER16_0);
}

void Timer_StopHostTimeout(void)
{
    Chip_TIMER_Disable(NSS_TIMER16_0);
    NVIC_DisableIRQ(CT16B0_IRQn);
    Chip_TIMER_ClearMatch(NSS_TIMER16_0, 0);
    Chip_TIMER_Reset(NSS_TIMER16_0);
    sHostTimeoutInterruptFired = false;
}

bool Timer_CheckHostTimeout(void)
{
    return sHostTimeoutInterruptFired;
}

/* -------------------------------------------------------------------------------- */

void Timer_StartFreeRunning(void)
{
    Chip_TIMER32_0_Init();
    Chip_TIMER_PrescaleSet(NSS_TIMER32_0, 1);
    Chip_TIMER_Reset(NSS_TIMER32_0);
    Chip_TIMER_Enable(NSS_TIMER32_0);
}

void Timer_StopFreeRunning(void)
{
    Chip_TIMER_Disable(NSS_TIMER32_0);
}

uint32_t Timer_GetFreeRunning(void)
{
    return Chip_TIMER_ReadCount(NSS_TIMER32_0);
}
