/*
 * Copyright 2016-2019 NXP
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

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

int ndeft2t_mod_example_3(uint8_t ** data)
{
//! [ndeft2t_mod_example_3]
    static uint8_t sBuffer[NFC_SHARED_MEM_BYTE_SIZE] __attribute__((aligned (4)));
    uint8_t instance[NDEFT2T_INSTANCE_SIZE] __attribute__((aligned (4)));
    NDEFT2T_PARSE_RECORD_INFO_T parseRecordInfo;
    int len = 0;
    uint8_t *payload = NULL;

    /* Chip_NFC_Init is normally already called during board initialization. */
    //Chip_NFC_Init(NSS_NFC);
    /* NDEFT2T_Init should have been called after NFC initialization. */
    //NDEFT2T_Init();

    /* In this example, we assume a proper NDEF message has already been written in the NFC shared memory by an external
     * reader (typically an NFC-enabled Android phone).
     */
    if (NDEFT2T_GetMessage(instance, sBuffer, NFC_SHARED_MEM_BYTE_SIZE)) {
        while (NDEFT2T_GetNextRecord(instance, &parseRecordInfo)) {
            if (parseRecordInfo.type == NDEFT2T_RECORD_TYPE_TEXT) {
                payload = (uint8_t *)NDEFT2T_GetRecordPayload(instance, &len);
                /* payload now points to the start of the record payload bytes.
                 * len contains the length of the payload in number of bytes.
                 */
                /* ... */
            }
        }
    }
//! [ndeft2t_mod_example_3]

    NDEFT2T_DeInit();
    Chip_NFC_DeInit(NSS_NFC);

    *data = payload;
    return len;
}
#pragma GCC diagnostic pop
