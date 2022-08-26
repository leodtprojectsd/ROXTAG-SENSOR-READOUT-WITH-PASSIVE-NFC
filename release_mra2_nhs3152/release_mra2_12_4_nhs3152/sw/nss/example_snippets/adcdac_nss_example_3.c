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

void adcdac_nss_example_3(void)
{
//! [adcdac_nss_example_3]
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_ANA0_5, IOCON_FUNC_1);
    Chip_ADCDAC_Init(NSS_ADCDAC0);
    Chip_ADCDAC_SetMuxADC(NSS_ADCDAC0, ADCDAC_IO_ANA0_5);
    Chip_ADCDAC_SetInputRangeADC(NSS_ADCDAC0, ADCDAC_INPUTRANGE_WIDE);
    Chip_ADCDAC_SetModeADC(NSS_ADCDAC0, ADCDAC_SINGLE_SHOT);
    Chip_ADCDAC_StartADC(NSS_ADCDAC0);

    while (!(Chip_ADCDAC_ReadStatus(NSS_ADCDAC0) & ADCDAC_STATUS_ADC_DONE)) {
        ; /* Wait until measurement completes. For single-shot mode only! */
    }

    int adcInput = Chip_ADCDAC_GetValueADC(NSS_ADCDAC0);
    /* Further handling of the result. */
    /* ... */
//! [adcdac_nss_example_3]
}

#pragma GCC diagnostic pop
