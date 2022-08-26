/*
 * Copyright 2015,2018 NXP
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

extern volatile int current_native;
extern volatile int current_picoampere;

void i2d_nss_example_1(void)
{
//! [i2d_nss_example_1]
    int i2dValue;
    int i2dNativeValue;
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_ANA0_4, IOCON_FUNC_1); /* Set pin function to analog */
    Chip_I2D_Init(NSS_I2D);
    Chip_I2D_Setup(NSS_I2D, I2D_SINGLE_SHOT, I2D_SCALER_GAIN_10_1, I2D_CONVERTER_GAIN_HIGH, 100);
    Chip_I2D_SetMuxInput(NSS_I2D, I2D_INPUT_ANA0_4);
    Chip_I2D_Start(NSS_I2D);
    while (!(Chip_I2D_ReadStatus(NSS_I2D) & I2D_STATUS_CONVERSION_DONE)) {
        ; /* wait */
    }
    i2dNativeValue = Chip_I2D_GetValue(NSS_I2D);
    i2dValue = Chip_I2D_NativeToPicoAmpere(i2dNativeValue, I2D_SCALER_GAIN_10_1, I2D_CONVERTER_GAIN_HIGH, 100);
    Chip_I2D_DeInit(NSS_I2D);
//! [i2d_nss_example_1]

    current_native = i2dNativeValue;
    current_picoampere = i2dValue;
}

#pragma GCC diagnostic pop
