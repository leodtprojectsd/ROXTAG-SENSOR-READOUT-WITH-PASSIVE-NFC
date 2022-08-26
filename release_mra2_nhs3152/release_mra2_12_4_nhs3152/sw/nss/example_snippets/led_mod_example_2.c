/*
 * Copyright 2016,2018-2019 NXP
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

void led_mod_example_2(void)
{
//! [led_mod_example_2]
    LED_Init(); /* Should already been taken care of during board initialization. */

    ASSERT(LED_GetState(LED_ALL) == 0);
    LED_On(LED_(0));
    ASSERT(LED_GetState(LED_(0)) == LED_(0));

    LED_On(LED_(1));
    ASSERT(LED_GetState(LED_(0) | LED_(1)) == (LED_(0) | LED_(1)));
//! [led_mod_example_2]
}
