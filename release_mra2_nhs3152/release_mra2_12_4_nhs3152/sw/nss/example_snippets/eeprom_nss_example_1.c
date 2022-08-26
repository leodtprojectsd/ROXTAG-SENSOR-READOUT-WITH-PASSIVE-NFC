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

void eeprom_nss_example_1(void)
{
//! [eeprom_nss_example_1]
    Chip_EEPROM_Init(NSS_EEPROM);

    /* Note: it is possible to read from and write to a non-aligned location and while crossing a row boundary: */
    int counter;
    Chip_EEPROM_Read(NSS_EEPROM, 61, &counter, sizeof(int));
    counter++;
    Chip_EEPROM_Write(NSS_EEPROM, 61, &counter, sizeof(int));

    Chip_EEPROM_Flush(NSS_EEPROM, true); /* After flush it is ensured that the new value is preserved. */

    /* Note: the previous flush call was optional since DeInit will perform it if required. */
    Chip_EEPROM_DeInit(NSS_EEPROM);
//! [eeprom_nss_example_1]
}

#pragma GCC diagnostic pop
