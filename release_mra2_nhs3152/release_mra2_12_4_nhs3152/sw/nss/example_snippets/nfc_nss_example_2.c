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

void nfc_nss_example_2(void)
{
//! [nfc_nss_example_2]
    uint8_t messageBuffer[NFC_SHARED_MEM_BYTE_SIZE];
    int msgLen;
    uint8_t *pMemPtr = (uint8_t*)NSS_NFC->BUF;

    Chip_NFC_Init(NSS_NFC);

    pMemPtr++; /* Skip NDEF TLV byte. */
    msgLen = *pMemPtr++; /* Extract the NDEF Message length */
    if (Chip_NFC_ByteRead(NSS_NFC, messageBuffer, pMemPtr, msgLen)) {
        /* Success. The message excluding the NDEF TLV header is available in messageBuffer and can now be further
         * processed.
         */
    }
    Chip_NFC_DeInit(NSS_NFC);
//! [nfc_nss_example_2]
}

#pragma GCC diagnostic pop
