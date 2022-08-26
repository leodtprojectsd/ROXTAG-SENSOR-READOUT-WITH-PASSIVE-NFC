/*
 * Copyright 2016-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "chip.h"
#include "led/led.h"

void led_mod_example_1(void)
{
    Chip_Clock_System_BusyWait_ms(1000);
//! [led_mod_example_1]
    LED_Init(); /* Should already been taken care of during board initialization. */

    /* Turn two LEDs on individually and then both off together */
    LED_On(LED_(0));
    LED_On(LED_(1));
    LED_Off(LED_(0) | LED_(1));

    /* Toggle two LEDs together */
    LED_SetState(LED_(0) | LED_(1), LED_(1) /* led 0 off, led 1 on */);
    LED_SetState(LED_(0) | LED_(1), LED_(0) /* led 0 on, led 1 off */);

    /* Toggle two LEDs together */
    LED_Toggle(LED_(0) | LED_(1));

    /* Turn all LEDs off */
    LED_Off(LED_ALL);
//! [led_mod_example_1]
}
