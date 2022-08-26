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

void iap_nss_example_1(uint8_t *buffer)
{
//! [iap_nss_example_1]
    uint32_t offset;
    uint32_t content;
    void *pFlash;
    IAP_STATUS_T result;

    result = Chip_IAP_Flash_PrepareSector(10, 10);
    ASSERT(result == IAP_STATUS_CMD_SUCCESS);

    result = Chip_IAP_Flash_EraseSector(10, 10, 8000);
    ASSERT(result == IAP_STATUS_CMD_SUCCESS);

    result = Chip_IAP_Flash_SectorBlankCheck(10, 10, &offset, &content); /* Optional */
    ASSERT(result == IAP_STATUS_CMD_SUCCESS);

    result = Chip_IAP_Flash_PrepareSector(10, 10);
    ASSERT(result == IAP_STATUS_CMD_SUCCESS);

    pFlash = (void *)10240; /* pFlash points to start address of sector 10 */
    result = Chip_IAP_Flash_Program(buffer, pFlash, 256, 8000);
    ASSERT(result == IAP_STATUS_CMD_SUCCESS);

    result = Chip_IAP_Compare(buffer, pFlash, 256, &offset); /* Optional */
    ASSERT(result == IAP_STATUS_CMD_SUCCESS);
//! [iap_nss_example_1]
}

#pragma GCC diagnostic pop
