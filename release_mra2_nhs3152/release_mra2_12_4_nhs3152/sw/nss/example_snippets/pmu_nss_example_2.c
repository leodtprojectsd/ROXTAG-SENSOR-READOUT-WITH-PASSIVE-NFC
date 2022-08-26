/*
 * Copyright 2015,2018-2019 NXP
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

volatile int counter = 0;

void pmu_nss_example_2(void)
{
    counter = 0;

//! [pmu_nss_example_2]
    Chip_SysCon_StartLogic_SetPIORisingEdge(SYSCON_STARTSOURCE_PIO0_1);
    Chip_SysCon_StartLogic_SetEnabledMask(SYSCON_STARTSOURCE_PIO0_1);
    Chip_SysCon_StartLogic_ClearStatus(SYSCON_STARTSOURCE_ALL);
    NVIC_EnableIRQ(PIO0_1_IRQn);
    while (1) {
        Chip_PMU_PowerMode_EnterDeepSleep();
        /* Further handling of the event "PIO1 pulled high / variable 'counter' incremented". */
        /* ... */
    }
//! [pmu_nss_example_2]
}

void pmu_nss_example_2_irq(void)
{
//! [pmu_nss_example_2_irq]
    /* To be called under interrupt from PIO0_1_IRQHandler */
    if ((Chip_SysCon_StartLogic_GetStatus() & SYSCON_STARTSOURCE_PIO0_1)) {
        Chip_SysCon_StartLogic_ClearStatus(SYSCON_STARTSOURCE_PIO0_1);
        counter++;
    }
//! [pmu_nss_example_2_irq]
}

#pragma GCC diagnostic pop
