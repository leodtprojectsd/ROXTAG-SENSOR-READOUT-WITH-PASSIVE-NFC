/*
 * Copyright 2015-2017 NXP
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

void nfc_nss_example_4(void)
{
//! [nfc_nss_example_4]
    Chip_NFC_Init(NSS_NFC);
    NVIC_EnableIRQ(NFC_IRQn);

    Chip_NFC_SetTargetAddress(NSS_NFC, 0); /* 32 bit aligned offset of location to be monitored */
    Chip_NFC_Int_SetEnabledMask(NSS_NFC, NFC_INT_TARGETWRITE | NFC_INT_TARGETREAD);
    while (true) {
        /* Further application logic goes here */
        /* ... */
    }
//! [nfc_nss_example_4]
}

void nfc_nss_example_4_irq(void)
{
//! [nfc_nss_example_4_irq]
    /* To be called under interrupt from NFC_IRQHandler */
    NFC_INT_T status = Chip_NFC_Int_GetRawStatus(NSS_NFC);
    if (status & NFC_INT_TARGETREAD) {
        /* Read access made to location specified in target register */
        /* Further handling of the interrupt. */
        /* ... */
    }
    if (status & NFC_INT_TARGETWRITE) {
        /* Write access made to location specified in target register */
        /* Further handling of the interrupt. */
        /* ... */
    }
    Chip_NFC_Int_ClearRawStatus(NSS_NFC, status);
//! [nfc_nss_example_4_irq]
}

#pragma GCC diagnostic pop
