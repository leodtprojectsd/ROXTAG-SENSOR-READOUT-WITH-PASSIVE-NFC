/*
 * Copyright 2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "mac/mac.h"

void mac_mod_example_1(void)
{
    //! [mac_mod_example_1]
    uint32_t key = {0x12345678, 0x12345678, 0x12345678, 0x12345678};
    uint32_t subkey1[4], subkey2[4];
    uint8_t message[] = "abcdefghijklmnopqrstuvwxyz012345";
    uint8_t signature[16];

    Mac_Init(subkey1, subkey2, key);
    Mac_Sign(signature, sizeof(signature), message, 32, key, subkey1, subkey2);
    //! [mac_mod_example_1]
}
