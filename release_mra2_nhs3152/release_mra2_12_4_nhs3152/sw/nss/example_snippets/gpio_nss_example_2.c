/*
 * Copyright 2015 NXP
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

void gpio_nss_example_2(void)
{
//! [gpio_nss_example_2]
    Chip_IOCON_Init(NSS_IOCON);
    Chip_IOCON_SetPinConfig(NSS_IOCON, 4, IOCON_FUNC_0 | IOCON_I2CMODE_PIO);
    Chip_IOCON_SetPinConfig(NSS_IOCON, 5, IOCON_FUNC_0 | IOCON_I2CMODE_PIO);
    Chip_IOCON_SetPinConfig(NSS_IOCON, 6, IOCON_FUNC_0 | IOCON_RMODE_INACT | IOCON_LPF_DISABLE);
    Chip_IOCON_SetPinConfig(NSS_IOCON, 7, IOCON_FUNC_0 | IOCON_RMODE_INACT | IOCON_LPF_DISABLE);

    Chip_GPIO_Init(NSS_GPIO);
    Chip_GPIO_SetPortDIROutput(
            NSS_GPIO, 0, NSS_GPIOn_PINMASK(4) | NSS_GPIOn_PINMASK(5) | NSS_GPIOn_PINMASK(6) | NSS_GPIOn_PINMASK(7));

    /* Operate Output. Only pins configured as output will be affected. */
    Chip_GPIO_SetPortValue(NSS_GPIO, 0, NSS_GPIOn_PINMASK(4) | NSS_GPIOn_PINMASK(4));
    Chip_GPIO_SetPortValue(NSS_GPIO, 0, NSS_GPIOn_PINMASK(5) | NSS_GPIOn_PINMASK(6));
    Chip_GPIO_SetPortValue(NSS_GPIO, 0, NSS_GPIOn_PINMASK(4) | NSS_GPIOn_PINMASK(7));
    Chip_GPIO_SetPortToggle(NSS_GPIO, 0,
                            NSS_GPIOn_PINMASK(4) | NSS_GPIOn_PINMASK(5) | NSS_GPIOn_PINMASK(6) | NSS_GPIOn_PINMASK(7));
    Chip_GPIO_SetPortToggle(NSS_GPIO, 0,
                            NSS_GPIOn_PINMASK(4) | NSS_GPIOn_PINMASK(5) | NSS_GPIOn_PINMASK(6) | NSS_GPIOn_PINMASK(7));
//! [gpio_nss_example_2]
}

#pragma GCC diagnostic pop
