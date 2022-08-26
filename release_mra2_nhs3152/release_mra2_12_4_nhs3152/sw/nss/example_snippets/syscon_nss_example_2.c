/*
 * Copyright 2015,2017 NXP
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
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

void syscon_nss_example_2(void)
{
//! [syscon_nss_example_2]
    static uint32_t ivt[48] __attribute__ ((aligned(1024)));
    memcpy(ivt, (uint32_t *)Chip_SysCon_IVT_GetAddress(), 4 * 48); /* Copy the entire IVT to SRAM */
    Chip_SysCon_IVT_SetAddress((uint32_t)ivt); /* Pass the IVT address as a value. */
    /* The IVT table in SRAM is now being used. */
//! [syscon_nss_example_2]
}

#pragma GCC diagnostic pop
