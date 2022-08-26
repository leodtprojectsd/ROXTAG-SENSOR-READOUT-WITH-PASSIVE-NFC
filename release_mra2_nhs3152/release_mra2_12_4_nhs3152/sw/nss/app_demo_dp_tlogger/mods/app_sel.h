/*
 * Copyright 2015-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __APP_SEL_H_
#define __APP_SEL_H_

#include <stdint.h>

#if !defined(SW_MAJOR_VERSION)
    #error SW_MAJOR_VERSION not defined. Define SW_MAJOR_VERSION in your Project settings.
    #error Under LPCXPresso: Project > Properties > C/C++ Build > Settings > Tool Settings > MCU C Compiler > Symbols
    #error Add a define similar to "SW_MAJOR_VERSION=1" (without surrounding quotes)
    #error Add this define to all build configurations.
#endif
// "SW_MAJOR_VERSION=$(shell date +%y%V)" to have it contain year and weeknumber: YYWW
// "SW_MAJOR_VERSION=$(shell date --utc +%s) to have it contain unix epoch time in seconds.

#if !defined(SW_MINOR_VERSION)
    #error SW_MINOR_VERSION not defined. Define SW_MINOR_VERSION in your Project settings.
    #error Under LPCXPresso: Project > Properties > C/C++ Build > Settings > Tool Settings > MCU C Compiler > Symbols
    #error Add a define similar to "SW_MINOR_VERSION=1" (without surrounding quotes)
    #error Add this define to all build configurations.
#endif

/* Diversities tweaking msg module for application-specific usage. */
#define MSG_APP_HANDLERS App_CmdHandler
#define MSG_APP_HANDLERS_COUNT 7U
#define MSG_RESPONSE_BUFFER_SIZE 20 /**< A value large enough to store #APP_MSG_RESPONSE_MEASURETEMPERATURE_T - nothing else is buffered. */
#define MSG_RESPONSE_BUFFER App_ResponseBuffer
#ifdef DEBUG
    #define MSG_ENABLE_RESET 1
    #define MSG_ENABLE_READREGISTER 1
    #define MSG_ENABLE_WRITEREGISTER 1
    #define MSG_ENABLE_READMEMORY 1
    #define MSG_ENABLE_WRITEMEMORY 1
    #define MSG_ENABLE_PREPAREDEBUG 1
#endif
#define MSG_ENABLE_CHECKBATTERY 1
#define MSG_COMMAND_ACCEPT_CB CommandAcceptCb

/* Diversities tweaking tmeas module for application-specific usage. */
#define TMEAS_CB App_TmeasCb

/* Diversities tweaking ndeft2t module for application-specific usage. */
#define NDEFT2T_EEPROM_COPY_SUPPPORT 0
#define NDEFT2T_FIELD_STATUS_CB App_FieldStatusCb
#define NDEFT2T_MSG_AVAILABLE_CB App_MsgAvailableCb
#define NDEFT2T_MSG_READ_CB App_MsgReadCb

/* Diversities tweaking storage module for application-specific usage. */
#define STORAGE_TYPE int16_t
#define STORAGE_BITSIZE 11 /**< round_up(log_2(2 * APP_MSG_MAX_TEMPERATURE)) */
#define STORAGE_SIGNED 1
#define STORAGE_EEPROM_FIRST_ROW 21
#define STORAGE_EEPROM_LAST_ROW (EEPROM_NR_OF_RW_ROWS - 1)
#define STORAGE_COMPRESS_CB App_CompressCb
#define STORAGE_DECOMPRESS_CB App_DecompressCb
#ifdef DEBUG
    #define STORAGE_FIRST_ALON_REGISTER 1
    #define STORAGE_WRITE_RECOVERY_EVERY_X_SAMPLES STORAGE_SAMPLE_ALON_CACHE_COUNT
    #define STORAGE_REDUCE_RECOVERY_WRITES 1
#else
    #define STORAGE_FIRST_ALON_REGISTER 3
    #define STORAGE_WRITE_RECOVERY_EVERY_X_SAMPLES STORAGE_SAMPLE_ALON_CACHE_COUNT
    #define STORAGE_REDUCE_RECOVERY_WRITES 0
#endif

/* Diversities tweaking event module for application-specific usage. */
#define EVENT_CB App_EventCb
#define EVENT_CB_OPENING_CALL 1
#define EVENT_CB_CLOSING_CALL 1
#define EVENT_EEPROM_FIRST_ROW 2
#define EVENT_EEPROM_LAST_ROW 20
#define EVENT_OVERHEAD_CHOICE EVENT_OVERHEAD_CHOICE_B

#ifdef DEBUG
    /* The diag module, if used, is compiled together with the chip library. It is also used at application level in
     * the msg module.
     * The define from lib_chip_nss: Project > Properties > C/C++ Build > Settings > Tool Settings > Preprocessor
     * is copied here to match expectations from the msg module with the chip library.
     */
    #define ENABLE_DIAG_MODULE 1
#else
    /* The release build configuration is linked with the Release_nodiag build configuration of the chip library. */
    #define ENABLE_DIAG_MODULE 0
#endif

/**
 * EEPROM SW Memory map
 * - Bytes [0 .. MEMORY_FIRSTUNUSEDEEPROMOFFSET[ in use by memory application file memory.c.
 * - Rows [EVENT_EEPROM_FIRST_ROW .. EVENT_EEPROM_LAST_ROW] in use by event module.
 * - Rows [STORAGE_EEPROM_FIRST_ROW .. STORAGE_EEPROM_LAST_ROW] in use by storage module.
 * .
 *
 * ALON register usage
 * - Words [0 .. ALON_WORD_SIZE[ in use by memory application file memory.c
 * - Words [STORAGE_FIRST_ALON_REGISTER .. 4] in use by module storage.
 * .
 */

#endif
