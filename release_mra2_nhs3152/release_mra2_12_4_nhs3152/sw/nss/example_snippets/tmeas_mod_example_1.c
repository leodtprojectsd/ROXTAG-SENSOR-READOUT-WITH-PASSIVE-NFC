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

#include "chip.h"
#include "tmeas/tmeas.h"

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

int tmeas_mod_example_1(void)
{
//! [tmeas_mod_example_1]
    int32_t temperature = (int32_t)TMeas_Measure(TSEN_10BITS, TMEAS_FORMAT_CELSIUS, true, 0);
    /* for 20.5 degrees Celsius, temperature will now equal 205 */
//! [tmeas_mod_example_1]
    return temperature;
}

#pragma GCC diagnostic pop
