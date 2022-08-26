/*
 * Copyright 2015 NXP
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

void timer_nss_example_2(void)
{
//! [timer_nss_example_2]
    Chip_TIMER32_0_Init();
    Chip_TIMER_PrescaleSet(NSS_TIMER32_0, ((uint32_t)Chip_Clock_System_GetClockFreq() / 500) - 1); /* TC tick increment at 500Hz */

    /* MR0 - 10ms toggle, No Interrupt, Don't stop, Don't reset */
    Chip_TIMER_SetMatch(NSS_TIMER32_0, 0, 5 - 1);
    Chip_TIMER_MatchDisableInt(NSS_TIMER32_0, 0);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER32_0, 0);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER32_0, 0);

    /* MR1 - Not in use. Set to greater than Cycle Length */
    Chip_TIMER_SetMatch(NSS_TIMER32_0, 1, 5);
    Chip_TIMER_MatchDisableInt(NSS_TIMER32_0, 1);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER32_0, 1);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER32_0, 1);

    /* MR2 -> Cycle Length 10ms */
    Chip_TIMER_SetMatch(NSS_TIMER32_0, 2, 5 - 1); /* 0-based MR: 0 to 4 (5 counts) */
    Chip_TIMER_MatchDisableInt(NSS_TIMER32_0, 2);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER32_0, 2);
    Chip_TIMER_ResetOnMatchEnable(NSS_TIMER32_0, 2);

    /* MR3 */
    Chip_TIMER_MatchDisableInt(NSS_TIMER32_0, 3);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER32_0, 3);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER32_0, 3);

    /* Enable EMR */
    Chip_TIMER_ExtMatchControlSet(NSS_TIMER32_0, 0, TIMER_EXTMATCH_TOGGLE, 0);
    Chip_TIMER_ExtMatchControlSet(NSS_TIMER32_0, 0, TIMER_EXTMATCH_DO_NOTHING, 1);

    /* Reset TC */
    Chip_TIMER_Reset(NSS_TIMER32_0);
    Chip_TIMER_Enable(NSS_TIMER32_0);
//! [timer_nss_example_2]
}

#pragma GCC diagnostic pop
