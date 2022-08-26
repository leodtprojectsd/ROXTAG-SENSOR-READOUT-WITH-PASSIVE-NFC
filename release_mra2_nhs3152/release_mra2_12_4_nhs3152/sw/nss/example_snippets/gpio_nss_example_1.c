/*
 * Copyright 2015,2017 NXP
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

void gpio_nss_example_1(void)
{
//! [gpio_nss_example_1]
    bool pin0state;
    bool pin1state;
    bool pin2state;
    bool pin3state;

    Chip_IOCON_Init(NSS_IOCON);
    Chip_IOCON_SetPinConfig(NSS_IOCON, 0, IOCON_FUNC_0 | IOCON_RMODE_INACT | IOCON_LPF_DISABLE);
    Chip_IOCON_SetPinConfig(NSS_IOCON, 1, IOCON_FUNC_0 | IOCON_RMODE_INACT | IOCON_LPF_DISABLE);
    Chip_IOCON_SetPinConfig(NSS_IOCON, 2, IOCON_FUNC_0 | IOCON_RMODE_INACT | IOCON_LPF_DISABLE);
    Chip_IOCON_SetPinConfig(NSS_IOCON, 4, IOCON_FUNC_0 | IOCON_I2CMODE_PIO);

    Chip_GPIO_Init(NSS_GPIO);
    Chip_GPIO_SetPortDIRInput(NSS_GPIO, 0, NSS_GPIOn_PINMASK(0) | NSS_GPIOn_PINMASK(1) | NSS_GPIOn_PINMASK(2));
    Chip_GPIO_SetPinDIROutput(NSS_GPIO, 0, 4);

    /* Setup interrupts. */
    NVIC_EnableIRQ(PIO0_IRQn);
    Chip_GPIO_SetupPinInt(NSS_GPIO, 0, 0, GPIO_INT_FALLING_EDGE);
    Chip_GPIO_SetupPinInt(NSS_GPIO, 0, 1, GPIO_INT_BOTH_EDGES);
    Chip_GPIO_SetupPinInt(NSS_GPIO, 0, 2, GPIO_INT_ACTIVE_HIGH_LEVEL);
    Chip_GPIO_EnableInt(NSS_GPIO, 0, NSS_GPIOn_PINMASK(0) | NSS_GPIOn_PINMASK(1) | NSS_GPIOn_PINMASK(2));

    /* Operate Output. */
    Chip_GPIO_SetPinOutHigh(NSS_GPIO, 0, 4);
    Chip_GPIO_SetPinOutLow(NSS_GPIO, 0, 4);
    Chip_GPIO_SetPinOutHigh(NSS_GPIO, 0, 4);
    Chip_GPIO_SetPinToggle(NSS_GPIO, 0, 4);
    Chip_GPIO_SetPinToggle(NSS_GPIO, 0, 4);

    /* Read Input. */
    pin0state = Chip_GPIO_GetPinState(NSS_GPIO, 0, 0);
    pin1state = Chip_GPIO_GetPinState(NSS_GPIO, 0, 1);
    pin2state = Chip_GPIO_GetPinState(NSS_GPIO, 0, 2);
//! [gpio_nss_example_1]
}

void gpio_nss_example_1_irq(void)
{
//! [gpio_nss_example_1_irq]
    /* To be called under interrupt from PIO0_IRQHandler */
    uint32_t states = Chip_GPIO_GetRawInts(NSS_GPIO, 0);
    if (states & NSS_GPIOn_PINMASK(0)) {
        /* Pin 0 falling edge event.
         * Further event handling below.
         */
        /* ... */
    }
    if (states & NSS_GPIOn_PINMASK(1)) {
        /* Pin 1 falling or rising edge event.
         * Further event handling below.
         */
        /* ... */
    }
    if (states & NSS_GPIOn_PINMASK(2)) {
        /* Pin 2 active high level-trigger
         * Further event handling below.
         */
        /* ... */
    }
    Chip_GPIO_ClearInts(NSS_GPIO, 0, states);
//! [gpio_nss_example_1_irq]
}

#pragma GCC diagnostic pop
