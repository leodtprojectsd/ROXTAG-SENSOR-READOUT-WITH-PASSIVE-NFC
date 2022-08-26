/*
 * Copyright 2016-2017,2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "chip.h"
#include "ndeft2t/ndeft2t.h"

void ndeft2t_mod_example_1(void)
{
//! [ndeft2t_mod_example_1]
    uint8_t instance[NDEFT2T_INSTANCE_SIZE] __attribute__((aligned (4)));
    uint8_t buffer[NFC_SHARED_MEM_BYTE_SIZE ] __attribute__((aligned (4)));
    uint8_t locale[] = "en";
    uint8_t text[] = "Hello World";

    Chip_NFC_Init(NSS_NFC); /* Is normally already called during board initialization. */
    NDEFT2T_Init();

    NDEFT2T_CreateMessage(instance, buffer, NFC_SHARED_MEM_BYTE_SIZE, true);

    NDEFT2T_CREATE_RECORD_INFO_T recordInfo = {.shortRecord = true, .pString = locale};
    if (NDEFT2T_CreateTextRecord(instance, &recordInfo)) {
        /* The payload length to pass excludes the NUL terminator. */
        if (NDEFT2T_WriteRecordPayload(instance, text, sizeof(text) - 1)) {
            NDEFT2T_CommitRecord(instance);
        }
    }

    NDEFT2T_CommitMessage(instance);
    /* The return value of the commit function is ignored here. */
//! [ndeft2t_mod_example_1]
}
