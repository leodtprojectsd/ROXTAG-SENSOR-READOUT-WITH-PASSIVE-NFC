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

#include "board.h"
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

void syscon_nss_example_1(void)
{
//! [syscon_nss_example_1]
    Chip_SysCon_Peripheral_DeassertReset(SYSCON_PERIPHERAL_RESET_I2C0);
    /* Next, enable I2C0 peripheral clock (see the clock driver). */
    /* ... */
    /* Reverse orde: first disable I2C0 peripheral clock */
    Chip_SysCon_Peripheral_AssertReset(SYSCON_PERIPHERAL_RESET_I2C0);

    Chip_SysCon_Peripheral_EnablePower(SYSCON_PERIPHERAL_POWER_I2D);
    /* Next, enable I2D peripheral clock (see to clock driver). */
    /* ... */
    /* Reverse order: first disable I2D peripheral clock. */
    Chip_SysCon_Peripheral_DisablePower(SYSCON_PERIPHERAL_POWER_I2D);
//! [syscon_nss_example_1]
}

#pragma GCC diagnostic pop
