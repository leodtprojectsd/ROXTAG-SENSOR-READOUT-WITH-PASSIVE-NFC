/*
 * Copyright 2015-2016,2020 NXP
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

void iocon_nss_example_1(void)
{
//! [iocon_nss_example_1]
    Chip_IOCON_Init(NSS_IOCON);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_0, IOCON_FUNC_0 | IOCON_RMODE_INACT | IOCON_LPF_DISABLE);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_1, IOCON_FUNC_0 | IOCON_RMODE_PULLUP | IOCON_LPF_ENABLE);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_2, IOCON_FUNC_1);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_3, IOCON_FUNC_0 | IOCON_RMODE_INACT | IOCON_LPF_DISABLE
                            | IOCON_CDRIVE_PROGRAMMABLECURRENT
                            | IOCON_ILO_VAL(4 * 255 / 20) /* 4.00mA low */
                            | IOCON_IHI_VAL(16 * 255 / 20) /* 16.0mA high */
                           );
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_4, IOCON_FUNC_1 | IOCON_I2CMODE_STDFAST);
//! [iocon_nss_example_1]
}

#pragma GCC diagnostic pop
