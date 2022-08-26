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
#pragma GCC diagnostic ignored "-Wunused-parameter"

//! [i2c_nss_example_2_data]
uint8_t slaveTxData[4] = {0xA8, 0x01, 0xC5, 0x9D};
uint8_t slaveRxData[4] = {0, 0, 0, 0};
//! [i2c_nss_example_2_data]

//! [i2c_nss_example_2_eventhandler]
void Example2_I2cSlaveEventHandler(I2C_ID_T id, I2C_EVENT_T event)
{
    /* Perform custom slave event handling */
}
//! [i2c_nss_example_2_eventhandler]

void i2c_nss_example_2(void)
{
//! [i2c_nss_example_2]
    Chip_IOCON_Init(NSS_IOCON); /* Is normally already called during board initialization. */
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_4, IOCON_FUNC_1 | IOCON_I2CMODE_STDFAST);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_5, IOCON_FUNC_1 | IOCON_I2CMODE_STDFAST);

    Chip_Clock_System_SetClockFreq(4000000);

    Chip_SysCon_Peripheral_DeassertReset(SYSCON_PERIPHERAL_RESET_I2C0);

    Chip_I2C_Init(I2C0);
    Chip_I2C_SetClockRate(I2C0, 100000);

    static I2C_XFER_T slaveXfer;
    slaveXfer.slaveAddr = 0x7F << 1;
    slaveXfer.rxBuff = &slaveRxData[0];
    slaveXfer.rxSz = sizeof(slaveRxData) + 1; /* +1: the address byte is counted but not copied to rxBuff */
    slaveXfer.txBuff = &slaveTxData[0];
    slaveXfer.txSz = sizeof(slaveTxData);

    Chip_I2C_SlaveSetup(I2C0, I2C_SLAVE_0, &slaveXfer, Example2_I2cSlaveEventHandler, 0);

    NVIC_EnableIRQ(I2C0_IRQn);
//! [i2c_nss_example_2]
}

#pragma GCC diagnostic pop
