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
#include <string.h>
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

void iap_nss_example_3(void)
{
//! [iap_nss_example_3]
    uint32_t partID = Chip_IAP_ReadPartID();
    int bootVersion = Chip_IAP_ReadBootVersion() & 0x0000FFFF; /* Mask off byte2 and byte3 */
    uint32_t uid[4];
    Chip_IAP_ReadUID(uid);
//! [iap_nss_example_3]
}

#pragma GCC diagnostic pop
