/*
 * Copyright 2015-2016,2019 NXP
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

//! [i2c_nss_example_1_data]
uint8_t masterTxData[4] = {0x31, 0x34, 0x61, 0x95};
uint8_t masterRxData[4] = {0, 0, 0, 0};
//! [i2c_nss_example_1_data]

void i2c_nss_example_1(void)
{
//! [i2c_nss_example_1]
    Chip_IOCON_Init(NSS_IOCON); /* Is normally already called during board initialization. */
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_4, IOCON_FUNC_1 | IOCON_I2CMODE_STDFAST);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_5, IOCON_FUNC_1 | IOCON_I2CMODE_STDFAST);

    Chip_Clock_System_SetClockFreq(4000000);

    Chip_SysCon_Peripheral_DeassertReset(SYSCON_PERIPHERAL_RESET_I2C0);

    Chip_I2C_Init(I2C0);
    Chip_I2C_SetClockRate(I2C0, 100000);

    Chip_I2C_SetMasterEventHandler(I2C0, Chip_I2C_EventHandler); /* Using the default I2C event handler */
    NVIC_EnableIRQ(I2C0_IRQn);

    I2C_XFER_T masterXfer;
    masterXfer.slaveAddr = 0x7F;
    masterXfer.rxBuff = masterRxData;
    masterXfer.rxSz = sizeof(masterRxData);
    masterXfer.txBuff = masterTxData;
    masterXfer.txSz = sizeof(masterTxData);

    Chip_I2C_MasterTransfer(I2C0, &masterXfer);
//! [i2c_nss_example_1]
}

//! [i2c_nss_irq_handler]
void I2C0_IRQHandler(void)
{
    if (Chip_I2C_IsMasterActive(I2C0)) {
        Chip_I2C_MasterStateHandler(I2C0);
    }
    else {
        Chip_I2C_SlaveStateHandler(I2C0);
    }
}
//! [i2c_nss_irq_handler]

#pragma GCC diagnostic pop
