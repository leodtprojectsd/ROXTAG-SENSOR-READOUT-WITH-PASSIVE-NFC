/*
 * Copyright 2015,2018-2019 NXP
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

void flash_nss_example_1_a(void)
{
    Chip_Clock_System_SetClockFreq(500 * 1000); /* Safeguard for the next call below, disabling high power mode. */
//! [flash_nss_example_1_a]
    Chip_Flash_SetHighPowerMode(false);
    Chip_Flash_SetNumWaitStates(1);
    Chip_Clock_System_SetClockFreq(8 * 1000 * 1000);
//! [flash_nss_example_1_a]
}

void flash_nss_example_1_b(void)
{
//! [flash_nss_example_1_b]
    Chip_Flash_SetHighPowerMode(true);
    Chip_Flash_SetNumWaitStates(0);
    Chip_Clock_System_SetClockFreq(8 * 1000 * 1000);
//! [flash_nss_example_1_b]
}

#pragma GCC diagnostic pop
