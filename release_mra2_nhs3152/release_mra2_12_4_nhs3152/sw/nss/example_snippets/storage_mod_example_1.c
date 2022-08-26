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

#include "board.h"
#include "storage/storage.h"
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wincompatible-pointer-types"
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

uint8_t gExample1_Output1;
uint8_t gExample1_Output10[10];

void doc_storage_example_1(void)
{
//! [storage_mod_example_1]
    int n;
    uint8_t one = 1;
    uint8_t ten[10] = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

    Chip_EEPROM_Init(NSS_EEPROM);
    Storage_Init();
    Storage_Reset(false);

    Storage_Write(&one, 1);
    Storage_Seek(0);

    n = Storage_Read(&one, 1); /* one will remain 1 */
    ASSERT(n == 1);

    Storage_Write(ten, 10);
    /*  Read index:    x
     * Data stored: 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
     * Write index:                                  x
     */

    Storage_Seek(0);
    n = Storage_Read(ten, 7); /* ten will become {1, 0, 1, 2, 3, 4, 5, 7, 8, 9} */
    ASSERT(n == 7);
    /*  Read index:                      x
     * Data stored: 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
     * Write index:                                  x
     */

    n = Storage_Read(ten, 100); /* ten will become {6, 7, 8, 9, 3, 4, 5, 7, 8, 9} */
    ASSERT(n == 4);
    /*  Read index:                                  x
     * Data stored: 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
     * Write index:                                  x
     */

    Storage_DeInit();
//! [storage_mod_example_1]

    gExample1_Output1 = one;
    for (int i = 0; i < 10; i++) {
        gExample1_Output10[i] = ten[i];
    }
}

#pragma GCC diagnostic pop
