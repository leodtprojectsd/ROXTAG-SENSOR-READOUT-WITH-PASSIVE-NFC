/*
 * Copyright 2016,2018 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include <string.h>
#include "board.h"
#include "uarttx/uarttx.h"

void uarttx_mod_example_1(void)
{
//! [uarttx_mod_example_1]
    UartTx_Init();

    /* Printing out 'He110 W0r1d!' in a very chopped up way. */
    UartTx_Tx((uint8_t *)"He", 2);
    UartTx_PrintDec(110, ' ');
    UartTx_PrintString("W");
    UartTx_PrintHex(0, 'r');
    UartTx_Printf("%d%s!", 1, 'd');

    UartTx_DeInit();
//! [uarttx_mod_example_1]
}
