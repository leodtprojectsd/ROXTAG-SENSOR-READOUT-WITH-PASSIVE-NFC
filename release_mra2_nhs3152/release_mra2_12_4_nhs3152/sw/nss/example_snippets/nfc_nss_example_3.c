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
#include <string.h>
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

static volatile bool sNfcAccessDetected;

void nfc_nss_example_3(void)
{
//! [nfc_nss_example_3]
    Board_Init();
    Chip_NFC_Init(NSS_NFC);
    NVIC_EnableIRQ(NFC_IRQn);
    Chip_NFC_Int_SetEnabledMask(NSS_NFC, NFC_INT_RFSELECT | NFC_INT_NFCOFF);

    sNfcAccessDetected = false;
    while (!sNfcAccessDetected) {
        ; /* wait */
    }
    LED_On(LED1);
    while (sNfcAccessDetected) {
        /* Further application logic goes here */
        /* ... */
    }
    LED_Off(LED1);

    Chip_NFC_Int_SetEnabledMask(NSS_NFC, NFC_INT_NONE);
    NVIC_DisableIRQ(NFC_IRQn);
    Chip_NFC_DeInit(NSS_NFC);
//! [nfc_nss_example_3]
}

void nfc_nss_example_3_irq(void)
{
//! [nfc_nss_example_3_irq]
    /* To be called under interrupt from NFC_IRQHandler */
    NFC_INT_T status = Chip_NFC_Int_GetRawStatus(NSS_NFC);
    if (status & NFC_INT_RFSELECT) {
        sNfcAccessDetected = true;
    }
    if (status & NFC_INT_NFCOFF) {
        sNfcAccessDetected = false;
    }
    Chip_NFC_Int_ClearRawStatus(NSS_NFC, status);
//! [nfc_nss_example_3_irq]
}

#pragma GCC diagnostic pop
