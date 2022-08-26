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

void timer_nss_example_3(void)
{
//! [timer_nss_example_3]
    Chip_TIMER16_0_Init();
    Chip_TIMER_PrescaleSet(NSS_TIMER16_0, ((uint32_t)Chip_Clock_System_GetClockFreq() / 250) - 1);

    /* MR0 -> low to high at 15, no interrupt, no stop, no reset */
    Chip_TIMER_SetMatch(NSS_TIMER16_0, 0, 15); /* 0-based MR. 15/125 -> 88% duty-cycle */
    Chip_TIMER_MatchDisableInt(NSS_TIMER16_0, 0);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 0);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER16_0, 0);

    /* MR1 -> low to high at 75, no interrupt, no stop, no reset */
    Chip_TIMER_SetMatch(NSS_TIMER16_0, 1, 75); /* 0-based MR. 75/125 -> 40% duty-cycle */
    Chip_TIMER_MatchDisableInt(NSS_TIMER16_0, 1);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 1);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER16_0, 1);

    /* MR2 -> PWM cycle time, no interrupt, no stop, reset on match */
    Chip_TIMER_SetMatch(NSS_TIMER16_0, 2, 125 - 1); /* 0-based MR: 0 to 124 (125 counts) */
    Chip_TIMER_MatchDisableInt(NSS_TIMER16_0, 2);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 2);
    Chip_TIMER_ResetOnMatchEnable(NSS_TIMER16_0, 2);

    /* MR3 */
    Chip_TIMER_MatchDisableInt(NSS_TIMER16_0, 3);
    Chip_TIMER_StopOnMatchDisable(NSS_TIMER16_0, 3);
    Chip_TIMER_ResetOnMatchDisable(NSS_TIMER16_0, 3);

    /* Enable PWM */
    Chip_TIMER_SetMatchOutputMode(NSS_TIMER16_0, 0, TIMER_MATCH_OUTPUT_PWM);
    Chip_TIMER_SetMatchOutputMode(NSS_TIMER16_0, 1, TIMER_MATCH_OUTPUT_PWM);

    /* Reset TC */
    Chip_TIMER_Reset(NSS_TIMER16_0);
    Chip_TIMER_Enable(NSS_TIMER16_0);
//! [timer_nss_example_3]
}

#pragma GCC diagnostic pop
