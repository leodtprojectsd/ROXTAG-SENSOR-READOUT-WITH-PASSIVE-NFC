/*
 * Copyright 2015,2017,2019 NXP
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

void syscon_nss_example_3(void)
{
//! [syscon_nss_example_3]
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_0, IOCON_FUNC_0);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_5, IOCON_FUNC_0);
    Chip_SysCon_StartLogic_SetPIORisingEdge(SYSCON_STARTSOURCE_PIO0_0);
    Chip_SysCon_StartLogic_SetEnabledMask(
            SYSCON_STARTSOURCE_PIO0_0 | SYSCON_STARTSOURCE_PIO0_5 | SYSCON_STARTSOURCE_NFC);
    Chip_SysCon_StartLogic_ClearStatus(SYSCON_STARTSOURCE_ALL);
    NVIC_EnableIRQ(PIO0_0_IRQn);
    NVIC_EnableIRQ(PIO0_5_IRQn);
    NVIC_EnableIRQ(RFFIELD_IRQn);
    /* The chip can now be set to Deep-Sleep mode (see PMU documentation for more details on Power Modes). */
//! [syscon_nss_example_3]
}

#pragma GCC diagnostic pop
