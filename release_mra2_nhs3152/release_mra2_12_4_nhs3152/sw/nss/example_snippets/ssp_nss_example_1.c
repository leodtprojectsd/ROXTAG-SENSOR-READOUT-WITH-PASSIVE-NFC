/*
 * Copyright 2016-2017,2019-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include <stdbool.h>
#include "board.h"
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

bool ssp_nss_example_1(void)
{
//! [ssp_nss_example_1]
    Chip_IOCON_Init(NSS_IOCON);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_2, IOCON_FUNC_1);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_6, IOCON_FUNC_1);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_8, IOCON_FUNC_1);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_9, IOCON_FUNC_1);

    /* Configure transfer parameters */
    Chip_SSP_Init(NSS_SSP0);
    Chip_SSP_SetMaster(NSS_SSP0, true);
    Chip_SSP_SetFormat(NSS_SSP0, SSP_BITS_8, SSP_FRAME_FORMAT_SPI, SSP_CLOCK_MODE0);
    Chip_SSP_SetBitRate(NSS_SSP0, 100000);

    /* Initialise rx/tx setup data structure */
    uint8_t tx[16];
    uint8_t rx[16];
    Chip_SSP_DATA_SETUP_T setup = {.rx_cnt = 0,
                                   .tx_cnt = 0,
                                   .length = sizeof(tx),
                                   .rx_data = rx,
                                   .tx_data = tx};

    Chip_SSP_Enable(NSS_SSP0);
    bool success = Chip_SSP_RWFrames_Blocking(NSS_SSP0, &setup) == sizeof(tx);
//! [ssp_nss_example_1]
    return success;
}

#pragma GCC diagnostic pop
