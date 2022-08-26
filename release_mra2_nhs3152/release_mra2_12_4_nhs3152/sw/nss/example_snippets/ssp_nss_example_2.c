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

#include "board.h"
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

bool isXferCompleted;

//! [ssp_nss_example_2_data]
/* Initialise rx/tx setup data structure */
uint8_t tx[16];
uint8_t rx[16];
Chip_SSP_DATA_SETUP_T setup = {.rx_cnt = 0,
                               .tx_cnt = 0,
                               .length = sizeof(tx),
                               .rx_data = rx,
                               .tx_data = tx};
//! [ssp_nss_example_2_data]

void ssp_nss_example_2(void)
{
    isXferCompleted = 0;
//! [ssp_nss_example_2]
    Chip_IOCON_Init(NSS_IOCON);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_2, IOCON_FUNC_1);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_6, IOCON_FUNC_1);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_8, IOCON_FUNC_1);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_9, IOCON_FUNC_1);

    /* Configure transfer parameters */
    Chip_SSP_Init(NSS_SSP0);
    Chip_SSP_SetMaster(NSS_SSP0, false);
    Chip_SSP_SetFormat(NSS_SSP0, SSP_BITS_8, SSP_FRAME_FORMAT_SPI, SSP_CLOCK_MODE0);
    Chip_SSP_SetBitRate(NSS_SSP0, 100000);
    Chip_SSP_ClearIntPending(NSS_SSP0, SSP_INT_CLEAR_BITMASK);

    Chip_SSP_Enable(NSS_SSP0);
    Chip_SSP_Int_Enable(NSS_SSP0);
    NVIC_EnableIRQ(SSP0_IRQn);
    while (!Chip_SSP_GetStatus(NSS_SSP0, SSP_STAT_TFE)) {
        ; /* wait */
    }

    Chip_SSP_Int_Disable(NSS_SSP0);
    NVIC_DisableIRQ(SSP0_IRQn);
//! [ssp_nss_example_2]

    isXferCompleted = 1;
}

void ssp_nss_example_2_irq(void)
{
//! [ssp_nss_example_2_irq]
    if ((Chip_SSP_Int_RWFrames8Bits(NSS_SSP0, &setup) == ERROR) ||
        ((setup.rx_cnt >= setup.length) && (setup.tx_cnt >= setup.length))) {
        Chip_SSP_Int_Disable(NSS_SSP0);
    }
//! [ssp_nss_example_2_irq]
}

#pragma GCC diagnostic pop
