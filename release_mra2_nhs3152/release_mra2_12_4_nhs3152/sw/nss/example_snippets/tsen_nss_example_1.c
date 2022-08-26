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

int doc_tsen_example_1(void)
{
    //! [tsen_nss_example_1]
    Chip_TSen_Init(NSS_TSEN);
    Chip_TSen_SetResolution(NSS_TSEN, TSEN_10BITS);
    Chip_TSen_Start(NSS_TSEN);
    while (!(Chip_TSen_ReadStatus(NSS_TSEN, NULL) & TSEN_STATUS_MEASUREMENT_DONE)) {
        ; /* Wait until the measurement has finished. */
    }
    int native = Chip_TSen_GetValue(NSS_TSEN);
    int value = Chip_TSen_NativeToCelsius(native, 1000); /* in millicelsius */
    Chip_TSen_DeInit(NSS_TSEN);
    //! [tsen_nss_example_1]
    return value;
}

#pragma GCC diagnostic pop
