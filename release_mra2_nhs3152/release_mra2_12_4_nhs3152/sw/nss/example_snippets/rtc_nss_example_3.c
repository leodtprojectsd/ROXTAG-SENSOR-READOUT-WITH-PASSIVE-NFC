/*
 * Copyright 2015-2017,2019 NXP
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

void rtc_nss_example_3(void)
{
//! [rtc_nss_example_3]
    if (Chip_PMU_PowerMode_GetDPDWakeupReason() == PMU_DPD_WAKEUPREASON_RTC) {
        /* Handling of the event "down-counter expired". */
        /* ... */
    }
    Chip_RTC_Init(NSS_RTC);
    Chip_RTC_Wakeup_SetControl(NSS_RTC, RTC_WAKEUPCTRL_ENABLE | RTC_WAKEUPCTRL_AUTO);
//! [rtc_nss_example_3]
}

void rtc_nss_example_3_b(void)
{
//! [rtc_nss_example_3_b]
    Chip_RTC_Wakeup_SetReload(NSS_RTC, 10);
    Chip_PMU_PowerMode_EnterDeepPowerDown(false);
    ASSERT(false); /* System does not resume here. Unreachable. */
//! [rtc_nss_example_3_b]
}

#pragma GCC diagnostic pop
