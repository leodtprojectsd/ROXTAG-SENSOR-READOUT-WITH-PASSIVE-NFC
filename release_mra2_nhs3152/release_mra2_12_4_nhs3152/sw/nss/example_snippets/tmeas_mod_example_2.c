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
#pragma GCC diagnostic ignored "-Wunused-parameter"

//! [App_TmeasCb]
/*#define TMEAS_CB App_TmeasCb*/ /* Uncomment and place this line of code in app_sel.h */
void App_TmeasCb(TSEN_RESOLUTION_T resolution, TMEAS_FORMAT_T format, uint32_t value, uint32_t context)
{
    /* In this example:
     * - resolution will equal TSEN_12BITS
     * - format will equal TMEAS_FORMAT_NATIVE
     * - for 20.5 degrees Celsius, value will now equal 0x44CD
     * - context will equal 42
     */
}
//! [App_TmeasCb]

int tmeas_mod_example_2(void)
{
//! [tmeas_mod_example_2]
    int errorcode = TMeas_Measure(TSEN_12BITS, TMEAS_FORMAT_NATIVE, false, 42);
    /* errorcode will now equal 0. Program continues, and
     * when the measurement is ready TmeasCb is called under interrupt.
     */
//! [tmeas_mod_example_2]
    return errorcode;
}

#pragma GCC diagnostic pop
