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

void led_mod_example_3(void)
{
    Chip_Clock_System_BusyWait_ms(1000);
//! [led_mod_example_3]
    LED_Init(); /* Should already been taken care of during board initialization. */

    /* growing */
    LED_Off(LED_ALL);
    for (int i = 0; i < LED_COUNT; i++) {
        LED_On(LED_(i));
    }
    LED_Off(LED_ALL);

    /* walking */
    for (int i = 0; i < LED_COUNT; i++) {
        LED_SetState(LED_ALL, LED_(i));
    }
//! [led_mod_example_3]
    LED_On(LED_ALL);
    LED_Off(LED_ALL);
}
