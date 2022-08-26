/*
 * Copyright 2019 NXP
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

static int sValue;

void doc_tsen_example_2_irq(void)
{
    //! [tsen_nss_example_2_irq]
    /* To be called under interrupt from TSEN_IRQHandler */
    int native;
    int value = 0;
    TSEN_INT_T rawStatus = Chip_TSen_Int_GetRawStatus(NSS_TSEN);
    TSEN_STATUS_T readStatus = Chip_TSen_ReadStatus(NSS_TSEN, NULL);
    if (rawStatus & (TSEN_INT_THRESHOLD_LOW | TSEN_INT_THRESHOLD_HIGH)) {
        if (readStatus & TSEN_STATUS_MEASUREMENT_DONE) {
            native = Chip_TSen_GetValue(NSS_TSEN);
            value = Chip_TSen_NativeToFahrenheit(native, 1000);
        }
    }
    Chip_TSen_Int_ClearRawStatus(NSS_TSEN, TSEN_INT_THRESHOLD_LOW | TSEN_INT_THRESHOLD_HIGH);
    Chip_TSen_Start(NSS_TSEN);
    //! [tsen_nss_example_2_irq]

    sValue = value;
}

int doc_tsen_example_2(void)
{
    //! [tsen_nss_example_2]
    Chip_TSen_Init(NSS_TSEN);
    Chip_TSen_SetResolution(NSS_TSEN, TSEN_12BITS);
    Chip_TSen_Int_SetThresholdLow(NSS_TSEN, Chip_TSen_FahrenheitToNative(14, 1)); /* T.Low: -10 C */
    Chip_TSen_Int_SetThresholdHigh(NSS_TSEN, Chip_TSen_FahrenheitToNative(23, 1)); /* T.High: -5 C */
    Chip_TSen_Int_SetEnabledMask(NSS_TSEN, TSEN_INT_THRESHOLD_LOW | TSEN_INT_THRESHOLD_HIGH);
    NVIC_EnableIRQ(TSEN_IRQn);
    Chip_TSen_Start(NSS_TSEN);
    //! [tsen_nss_example_2]

    Chip_Clock_System_BusyWait_ms(123);
    Chip_TSen_DeInit(NSS_TSEN);
    return sValue;
}

#pragma GCC diagnostic pop
