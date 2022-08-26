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

void i2d_nss_example_2(void)
{
//! [i2d_nss_example_2]
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_ANA0_5, IOCON_FUNC_1);
    Chip_I2D_Init(NSS_I2D);
    Chip_I2D_Setup(NSS_I2D, I2D_CONTINUOUS, I2D_SCALER_GAIN_1_1, I2D_CONVERTER_GAIN_LOW, 10);
    Chip_I2D_SetMuxInput(NSS_I2D, I2D_INPUT_ANA0_5);
    int threshold = Chip_I2D_PicoAmpereToNative(1 * 1000 * 1000, I2D_SCALER_GAIN_1_1, I2D_CONVERTER_GAIN_LOW, 10);
    Chip_I2D_Int_SetThresholdLow(NSS_I2D, threshold);
    Chip_I2D_Int_SetEnabledMask(NSS_I2D, I2D_INT_THRESHOLD_LOW);
    NVIC_EnableIRQ(I2D_IRQn); /* I2D Interrupt line enabled in the NVIC keeping default priority settings */
    Chip_I2D_Start(NSS_I2D);
//! [i2d_nss_example_2]
}

void i2d_nss_example_2_irq(void)
{
//! [i2d_nss_example_2_irq]
    /* To be called under interrupt from I2D_IRQHandler */
    int i2dValue = 0;
    int i2dNativeValue = 0;
    if ((Chip_I2D_Int_GetRawStatus(NSS_I2D) & I2D_INT_THRESHOLD_LOW)) {
        if ((Chip_I2D_ReadStatus(NSS_I2D) & I2D_STATUS_CONVERSION_DONE)) {
            /* No need to check interrupt flag here, as I2D_STATUS_CONVERSION_DONE status flag holds the same meaning. */
            i2dNativeValue = Chip_I2D_GetValue(NSS_I2D);
            i2dValue = Chip_I2D_NativeToPicoAmpere(current_native, I2D_SCALER_GAIN_1_1, I2D_CONVERTER_GAIN_LOW, 10);
        }
        Chip_I2D_Int_ClearRawStatus(NSS_I2D, I2D_INT_THRESHOLD_LOW);
    }
//! [i2d_nss_example_2_irq]
    current_native = i2dNativeValue;
    current_picoampere = i2dValue;

}

#pragma GCC diagnostic pop
