/*
 * Copyright 2016-2017 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include <string.h>
#include "chip.h"
#include "compress/compress.h"

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

int resultOfMemcmp = -1; /* any value which memcmp doesn't return will do. */
int sizeAfterEncoding = -1;
int sizeAfterDecoding = -1;

void compress_mod_example_1(void)
{
//! [compress_mod_example_1]
    uint8_t data[1234];
    /* ... fill in 1234 bytes of data ... */

    /* Compression is a one function call: */
    uint8_t compressed[1234];
    int compressedSize = Compress_Encode(data, 1234, compressed, 1234);
    /* compressedSize is now a number less than 1234.*/

    /* Retrieving the original data back is again a one function call: */
    uint8_t uncompressed[1234];
    int uncompressedSize = Compress_Decode(compressed, compressedSize, uncompressed, 1234);
    /* uncompressedSize is now equal to 1234.*/
    /* all bytes in uncompressed are now identical to the bytes in data.*/
//! [compress_mod_example_1]
    sizeAfterEncoding = compressedSize;
    sizeAfterDecoding = uncompressedSize;
    resultOfMemcmp = memcmp(data, uncompressed, sizeof(data));
}

#pragma GCC diagnostic pop
