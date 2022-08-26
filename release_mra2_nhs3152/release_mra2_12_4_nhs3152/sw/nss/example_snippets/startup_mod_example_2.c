/*
 * Copyright 2016-2017,2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"

//! [startup_mod_example_2_hardfault]
void HardFault_Handler(void)
{
    ASSERT(false);
}
//! [startup_mod_example_2_hardfault]

void startup_mod_example_2_main(void)
{
//! [startup_mod_example_2_main]
    ((void(*)(void))40000)(); /* assumption: 40000 is not a valid address */
//! [startup_mod_example_2_main]
}
