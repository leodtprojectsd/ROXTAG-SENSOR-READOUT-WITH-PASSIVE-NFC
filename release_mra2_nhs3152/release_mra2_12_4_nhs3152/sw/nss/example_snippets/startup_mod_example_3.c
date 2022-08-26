/*
 * Copyright 2016,2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"

static volatile bool sMyPreMainIsCalled = false;

static void MyPreMain(void)
{
    sMyPreMainIsCalled = true;
}

//! [startup_mod_example_3_resetisr]
void ResetISR(void)
{
    Startup_VarInit(); /* initialization of static variables */

    MyPreMain();

    extern int main(void);
    main();
}
//! [startup_mod_example_3_resetisr]
