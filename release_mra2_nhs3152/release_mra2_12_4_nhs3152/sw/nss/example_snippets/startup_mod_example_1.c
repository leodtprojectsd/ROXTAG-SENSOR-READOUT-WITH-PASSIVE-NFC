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

//! [startup_mod_example_1_ct16b0]
void CT16B0_IRQHandler(void)
{
    LED_Toggle(LED_(0));
    Chip_TIMER_ClearMatch(NSS_TIMER16_0, 0);
}
//! [startup_mod_example_1_ct16b0]

void startup_mod_example_1_main(void)
{
//! [startup_mod_example_1_main]
    Board_Init();

    Chip_TIMER16_0_Init();
    /* Set the pre-scaler of the timer to count in milliseconds */
    Chip_TIMER_PrescaleSet(NSS_TIMER16_0, (uint32_t)Chip_Clock_System_GetClockFreq()/1000 - 1);

    /* We set up a match register 0 to generate an interrupt and reset timer */
    Chip_TIMER_SetMatch(NSS_TIMER16_0, 0, 499);
    Chip_TIMER_MatchEnableInt(NSS_TIMER16_0, 0);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 0);
    Chip_TIMER_ResetOnMatchEnable(NSS_TIMER16_0, 0);

    NVIC_EnableIRQ(CT16B0_IRQn);

    Chip_TIMER_Reset(NSS_TIMER16_0);
    Chip_TIMER_Enable(NSS_TIMER16_0);
//! [startup_mod_example_1_main]
}
