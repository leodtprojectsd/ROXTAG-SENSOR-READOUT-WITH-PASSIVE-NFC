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

volatile int temperature = 0;

void pmu_nss_example_1(void)
{
    temperature = 0;

//! [pmu_nss_example_1]
    Chip_TSen_Init(NSS_TSEN);
    Chip_TSen_Int_SetEnabledMask(NSS_TSEN, TSEN_INT_MEASUREMENT_RDY);
    NVIC_EnableIRQ(TSEN_IRQn);
    Chip_TSen_Start(NSS_TSEN);
    Chip_PMU_PowerMode_EnterSleep();
    /* Code execution resumes here after the ISR has been serviced. */
//! [pmu_nss_example_1]

    {
        __asm__("BKPT");
    }
}

void pmu_nss_example_1_irq(void)
{
//! [pmu_nss_example_1_irq]
    /* To be called under interrupt from TSEN_IRQHandler */
    if ((Chip_TSen_Int_GetRawStatus(NSS_TSEN) & TSEN_INT_MEASUREMENT_RDY)) {
        Chip_TSen_Int_ClearRawStatus(NSS_TSEN, TSEN_INT_MEASUREMENT_RDY);
        int native = Chip_TSen_GetValue(NSS_TSEN);
        temperature = Chip_TSen_NativeToCelsius(native, 10);
        /* If the temperature is 21.2C, the variable "temperature" will be equal to 212. */
    }
//! [pmu_nss_example_1_irq]
}

#pragma GCC diagnostic pop
