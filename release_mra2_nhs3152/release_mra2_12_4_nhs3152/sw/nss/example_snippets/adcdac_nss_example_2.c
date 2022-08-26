/*
 * Copyright 2015,2017-2018 NXP
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

void adcdac_nss_example_2(void)
{
//! [adcdac_nss_example_2]
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_ANA0_5, IOCON_FUNC_1);
    Chip_ADCDAC_Init(NSS_ADCDAC0);
    Chip_ADCDAC_SetMuxADC(NSS_ADCDAC0, ADCDAC_IO_ANA0_5);
    Chip_ADCDAC_SetInputRangeADC(NSS_ADCDAC0, ADCDAC_INPUTRANGE_WIDE);

    Chip_ADCDAC_Int_SetThresholdLowADC(NSS_ADCDAC0, 1138);
    Chip_ADCDAC_Int_SetThresholdHighADC(NSS_ADCDAC0, 3412);

    Chip_ADCDAC_SetModeADC(NSS_ADCDAC0, ADCDAC_CONTINUOUS);
    Chip_ADCDAC_Int_SetEnabledMask(NSS_ADCDAC0, (ADCDAC_INT_T)(ADCDAC_INT_THRESHOLD_LOW_ADC | ADCDAC_INT_THRESHOLD_HIGH_ADC));
    NVIC_EnableIRQ(ADCDAC_IRQn);
    Chip_ADCDAC_StartADC(NSS_ADCDAC0);
//! [adcdac_nss_example_2]
}

void adcdac_nss_example_2_irq(void)
{
//! [adcdac_nss_example_2_irq]
    /* To be called under interrupt from ADC_IRQHandler */
    ADCDAC_INT_T status;
    int adcInput;
    status = Chip_ADCDAC_Int_GetRawStatus(NSS_ADCDAC0);
    Chip_ADCDAC_Int_ClearRawStatus(NSS_ADCDAC0, status);

    if (status & (ADCDAC_INT_THRESHOLD_LOW_ADC | ADCDAC_INT_THRESHOLD_HIGH_ADC)) {
        Chip_ADCDAC_Int_SetEnabledMask(NSS_ADCDAC0, (ADCDAC_INT_NONE));
        adcInput = Chip_ADCDAC_GetValueADC(NSS_ADCDAC0);
        /* Further handling of the threshold breach. */
        /* ... */
    }
//! [adcdac_nss_example_2_irq]
}

#pragma GCC diagnostic pop
