/*
 * Copyright 2017-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"
#include "../../mods/i2cbbm/i2cbbm.h"

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

int i2cbbm_mod_example_1(void)
{
    int frequency = 4000000;
    Chip_Flash_SetHighPowerMode(frequency == 8000000);
    Chip_Clock_System_SetClockFreq(frequency);

//! [i2cbbm_mod_example_1]
    const uint8_t data[4] = {0x31, 0x34, 0x61, 0x95};
    uint8_t slaveAddress = 0x7F; /* 7-bit I2C address. MSBit is disregarded. */

    I2cbbm_Init();
    I2cbbm_SetAddress(slaveAddress);

    int written = I2cbbm_Write(data, 4);

    I2cbbm_DeInit();
//! [i2cbbm_mod_example_1]

    return written;
}

#pragma GCC diagnostic pop
