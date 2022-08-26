/*
 * Copyright 2018-2020 NXP
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
#include "diag.h"

#if ENABLE_DIAG_MODULE

#define ENDURANCE_LIMIT 10000
#define DIAG_EEPROM_ROW EEPROM_NR_OF_RW_ROWS /* This row is R/W: see UM10876 v2.04, 17.2 On-chip EEPROM */
#define DIAG_EEPROM_START_OFFSET (EEPROM_ROW_SIZE * DIAG_EEPROM_ROW)

#ifndef DIAG_HEADER
    #define DIAG_HEADER 0x42 /**< A very random number which is assumed to not occur by accident. */
#endif

/** All data stored in EEPROM. */
typedef struct WORKSPACE_S {
    uint8_t header; /**< Must equal #DIAG_HEADER, or the other fields below are not valid. */

    /**
     * Initialized in #Diag_Init, used in #Diag_NFC_IRQHandler.
     * @note Does not require to be stored in EEPROM - doesn't hurt either.
     * @note To be used as a bool - using an uint8_t reduces code size.
     */
    bool isSelected;

    /**
     * A copy of the RTC up timer.
     * Used for two purposes:
     * - Set in #Diag_Init, read in #Diag_DeInit, to track the Active time.
     * - Set in #Diag_DeInit, read in #Diag_Init, to track the Deep Power Down time.
     * .
     * Updated in Diag_TrackRtcUpdate, to compensate for RTC up timer reconfiguration.
     */
    int lastStoredRtc;

    /**
     * Initialized in #Diag_Init, written to and read from in #Diag_NFC_IRQHandler.
     * @note Does not require to be stored in EEPROM - doesn't hurt either.
     */
    int lastTapTime;

    DIAG_DATA_T data; /**< Statistics exposed via #Diag_Get */
} WORKSPACE_T;

__attribute__ ((section(".noinit")))
static WORKSPACE_T sWorkspace; /**< Copy in SRAM of EEPROM diagnostics data. */

/* ------------------------------------------------------------------------- */

/* NFC IRQ hook. */
void Diag_NFC_IRQHandler(void)
{
    /* This IRQ hook only checks NFC field status only. No IRQ handling or clearing is done: This is left for the
     * overridden implementation of NFC_IRQHandler.
     * Translate the NFC_INT_NFCOFF and NFC_INT_RFSELECT interrupts to a boolean value to indicate the status of the
     * NFC field. Care must be taken since two interrupts can be set simultaneously.
     * The value of the NFC status register is used as the 'truth'.
     */
    NFC_INT_T nfcRawInterruptStatus = Chip_NFC_Int_GetRawStatus(NSS_NFC);
    NFC_STATUS_T nfcStatus = Chip_NFC_GetStatus(NSS_NFC);
    bool selected = (nfcStatus & NFC_STATUS_SEL) != 0;

    if (nfcRawInterruptStatus & (NFC_INT_RFSELECT | NFC_INT_NFCOFF)) {
        int now = Chip_RTC_Time_GetValue(NSS_RTC);
        int diff = now - sWorkspace.lastTapTime;
        /* Only count new select states when they are more than 1 second apart. This absorbs the weird behavior
         * of the Android NFC OS driver/middleware, which selects a tag and powers off the NFC field multiple times
         * in its process to determine the correct NFC protocol to follow.
         */
        if (diff > 1) {
            if (selected && !sWorkspace.isSelected) {
                sWorkspace.data.nfcTapCount++;
            }
            sWorkspace.isSelected = selected;
        }
    }

    /* Call original weak handler if it was defined (overridden).
     * Use Reserved1 IRQ (EXCEPTION39) value as defaultIntHandler pointer for comparison.
     */
    extern void (* const g_pfnVectors[])(void);
    if(NFC_IRQHandler != g_pfnVectors[39]) {
        NFC_IRQHandler();
    }
}

/* ------------------------------------------------------------------------- */

void Diag_Init(void)
{
    /* This call is expected before variables from BSS and DATA sections have been initialized. Take care not to rely
     * on them during this call.
     */

    Chip_EEPROM_Init(NSS_EEPROM); /* Ensure EEPROM is available. */
    Chip_EEPROM_Read(NSS_EEPROM, DIAG_EEPROM_START_OFFSET, &sWorkspace, sizeof(WORKSPACE_T));
    if (sWorkspace.header != DIAG_HEADER)
    {
        /* No valid data in EEPROM. Reset data. */
        memset(&sWorkspace, 0, sizeof(WORKSPACE_T));
        sWorkspace.header = DIAG_HEADER;
    }

    int now = Chip_RTC_Time_GetValue(NSS_RTC);
    int diff = now - sWorkspace.lastStoredRtc;
    if (diff <= 0) { /* Can only happen when the RTC got reset, i.e. after a power-off or a hard reset. */
        sWorkspace.data.coldBootCount++;
    }
    else {
        sWorkspace.data.wakeUpCount++;
        sWorkspace.data.deepPowerDownTime += diff;
    }
    sWorkspace.lastStoredRtc = now;
    sWorkspace.lastTapTime = -1;
}

void Diag_DeInit(void)
{
    if (sWorkspace.data.coldBootCount + sWorkspace.data.wakeUpCount < ENDURANCE_LIMIT) {
        int now = Chip_RTC_Time_GetValue(NSS_RTC);
        int diff = now - sWorkspace.lastStoredRtc;
        sWorkspace.data.activeTime += diff;
        sWorkspace.lastStoredRtc = now;

        Chip_EEPROM_Init(NSS_EEPROM); /* Ensure EEPROM is available. */
        Chip_EEPROM_Write(NSS_EEPROM, DIAG_EEPROM_START_OFFSET, &sWorkspace, sizeof(WORKSPACE_T));
        Chip_EEPROM_Flush(NSS_EEPROM, true);
    }
}

void Diag_TrackRtcUpdate(int new)
{
    int old = Chip_RTC_Time_GetValue(NSS_RTC);
    sWorkspace.lastStoredRtc -= old;
    sWorkspace.lastStoredRtc += new;
}

const DIAG_DATA_T * Diag_Get(void)
{
    return &sWorkspace.data;
}

#endif
