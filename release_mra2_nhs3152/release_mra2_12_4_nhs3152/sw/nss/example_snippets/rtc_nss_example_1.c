/*
 * Copyright 2015,2017,2019 NXP
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

void rtc_nss_example_1(void)
{
//! [rtc_nss_example_1]
    Chip_RTC_Init(NSS_RTC);
    Chip_RTC_Wakeup_SetControl(NSS_RTC, RTC_WAKEUPCTRL_ENABLE | RTC_WAKEUPCTRL_AUTO);
    Chip_RTC_Int_SetEnabledMask(NSS_RTC, RTC_INT_WAKEUP);
    NVIC_EnableIRQ(RTC_IRQn);
//! [rtc_nss_example_1]
}

void rtc_nss_example_1_b(void)
{
//! [rtc_nss_example_1_b]
    Chip_RTC_Wakeup_SetReload(NSS_RTC, 10);
    Chip_PMU_PowerMode_EnterSleep();
    /* RTC_IRQHandler is called after 10 seconds. System resumes here after ISR has been serviced. */
    /* ... */
//! [rtc_nss_example_1_b]
}

void rtc_nss_example_1_irq(void)
{
//! [rtc_nss_example_1_irq]
    /* To be called under interrupt from RTC_IRQHandler */
    if (Chip_RTC_Int_GetRawStatus(NSS_RTC) & RTC_INT_WAKEUP) {
        Chip_RTC_Int_ClearRawStatus(NSS_RTC, RTC_INT_WAKEUP);
        /* Further handling of the event "down-counter expired". */
        /* ... */
    }
//! [rtc_nss_example_1_irq]
}

#pragma GCC diagnostic pop
