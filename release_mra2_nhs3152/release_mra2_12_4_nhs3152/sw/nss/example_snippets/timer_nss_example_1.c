/*
 * Copyright 2015,2017 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

uint32_t counterTick_10ms = 0;

void timer_nss_example_1_irq(void)
{
//! [timer_nss_example_1_irq]
    /* To be called under interrupt from CT16B0_IRQHandler */
    extern uint32_t counterTick_10ms;

    if (Chip_TIMER_MatchPending(NSS_TIMER16_0, 0)) {
        Chip_TIMER_ClearMatch(NSS_TIMER16_0, 0);

        counterTick_10ms++;
        /* Further handling of the timer interrupt */
        /* ... */
    }
//! [timer_nss_example_1_irq]
}

void timer_nss_example_1(void)
{
//! [timer_nss_example_1]
    Chip_TIMER16_0_Init();
    NVIC_EnableIRQ(CT16B0_IRQn);
    Chip_TIMER_PrescaleSet(NSS_TIMER16_0, ((uint32_t)Chip_Clock_System_GetClockFreq() / 500) - 1 /* -1 for 0-based PC */);

    /* MR0 */
    Chip_TIMER_SetMatch(NSS_TIMER16_0, 0, 5 - 1); /* 0-based MR. (5x 2ms = 10ms) */
    Chip_TIMER_MatchEnableInt(NSS_TIMER16_0, 0);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 0);
    Chip_TIMER_ResetOnMatchEnable(NSS_TIMER16_0, 0);

    /* MR1 */
    Chip_TIMER_MatchDisableInt(NSS_TIMER16_0, 1);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 1);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER16_0, 1);

    /* MR2 */
    Chip_TIMER_MatchDisableInt(NSS_TIMER16_0, 2);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 2);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER16_0, 2);

    /* MR3 */
    Chip_TIMER_MatchDisableInt(NSS_TIMER16_0, 3);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 3);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER16_0, 3);

    /* Reset TC */
    Chip_TIMER_Reset(NSS_TIMER16_0);
    Chip_TIMER_Enable(NSS_TIMER16_0);
//! [timer_nss_example_1]
}

#pragma GCC diagnostic pop
