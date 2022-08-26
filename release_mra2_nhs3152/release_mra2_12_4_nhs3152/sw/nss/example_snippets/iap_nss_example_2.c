/*
 * Copyright 2015-2016,2018 NXP
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

void iap_nss_example_2(void)
{
//! [iap_nss_example_2]
    uint32_t rtcCal = Chip_IAP_ReadFactorySettings((uint32_t) &NSS_RTC->CAL);
    Chip_RTC_SetCalibration(NSS_RTC, (int)rtcCal);
//! [iap_nss_example_2]
}

#pragma GCC diagnostic pop
