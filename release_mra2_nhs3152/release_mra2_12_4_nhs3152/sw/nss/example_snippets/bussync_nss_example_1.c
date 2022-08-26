/*
 * Copyright 2015,2019 NXP
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

uint32_t bussync_nss_example_1(void)
{
//! [bussync_nss_example_1]
    /* The access counter to be used by BusSync for synchronizing access to RTC - since RTC is a slow HW block. */
    static volatile int sAccessCounter = 0;
    Chip_BusSync_WriteReg(&NSS_RTC->ACCSTAT, &sAccessCounter, &NSS_RTC->CR, 0);
    uint32_t value = Chip_BusSync_ReadReg(&NSS_RTC->ACCSTAT, &sAccessCounter, &NSS_RTC->SR);
//! [bussync_nss_example_1]
    return value;
}

#pragma GCC diagnostic pop
