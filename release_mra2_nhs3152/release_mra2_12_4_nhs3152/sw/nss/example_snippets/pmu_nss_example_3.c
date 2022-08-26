/*
 * Copyright 2015-2016,2018-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include <string.h>
#include "board.h"
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

void pmu_nss_example_3(void)
{
//! [pmu_nss_example_3]
    uint32_t counters[4];
    Chip_PMU_GetRetainedData(counters, 0, 4);
    
    PMU_DPD_WAKEUPREASON_T reason = Chip_PMU_PowerMode_GetDPDWakeupReason();
    if (reason == PMU_DPD_WAKEUPREASON_NONE) {
        /* POR or RESETN pin asserted - (re-)setup and (re-)initialize */
        memset(counters, 0, sizeof(counters));
        Chip_PMU_SetWakeupPinEnabled(true);
    }
    else {
        ASSERT(reason < 4);
        /* PMU_DPD_WAKEUPREASON_NFCPOWER: Waking up from Deep Power Down due to the presence of an NFC field */
        /* PMU_DPD_WAKEUPREASON_WAKEUPPIN: Waking up from Deep Power Down due to Wakeup pin toggled low */
        /* PMU_DPD_WAKEUPREASON_RTC: Waking up from Deep Power Down due to RTC down counter expired */
        counters[reason]++;
    }
    
    Chip_PMU_SetRetainedData(counters, 0, 4);
    Chip_Clock_System_BusyWait_ms(4000); /* Give a little time for a possible debugger to break in. */
    Chip_PMU_PowerMode_EnterDeepPowerDown(false);
//! [pmu_nss_example_3]
}

#pragma GCC diagnostic pop
