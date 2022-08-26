/*
 * Copyright 2016-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include <string.h>
#include "board.h"
#include "storage/storage.h"
#include "event/event.h"
#include "event_tag.h"
#include "msghandler_protocol.h"
#include "validate.h"
#include "memory.h"

/* ------------------------------------------------------------------------- */

#define ALON_WORD_SIZE ((sizeof(ALON_T) + (sizeof(uint32_t) - 1)) / sizeof(uint32_t))

#define EEPROM_OFFSET_BUILDTIMESTAMP 0
#define EEPROM_OFFSET_CONFIG (EEPROM_OFFSET_BUILDTIMESTAMP + sizeof(uint32_t))
#define EEPROM_OFFSET_END (EEPROM_OFFSET_CONFIG + sizeof(MEMORY_CONFIG_T))

/* ------------------------------------------------------------------------- */

/** Defines what is stored in the GP registers that reside in the PMU always-on domain. */
typedef struct ALON_S {
    /**
     * Flag that indicates measurements can still continue.
     * New measurements cannot be taken when either the battery died, or when storage space is full.
     * - battery:
     *  During normal lifetime, this flag is set when configuring the IC and checked to decide to power-off - and make
     *  no (new) measurements or to go to Deep Power Down - and make a new measurements after the timer expired. When it
     *  is zero while checking, it indicates we lost track of time, and cannot but go back to the power-off mode - a
     *  reconfiguration and a full read-out is still possible, but storing new samples is not allowed any more.
     * - storage:
     *  When EEPROM is completely filled, but moving data from EEPROM to FLASH is no longer possible, new measurements
     *  can be taken, but cannot be stored anymore. For this demo application, it is chosen to stop measurements
     *  altogether. A different choice might be to continue measurements, not store them, but still use them for
     *  decision taking on validity of the attached product.
     */
    bool uninterrupted;
} ALON_T;

/* ------------------------------------------------------------------------- */

#if !defined(APP_BUILD_TIMESTAMP)
    #error APP_BUILD_TIMESTAMP not defined. Define APP_BUILD_TIMESTAMP in your Project settings.
    #error Under LPCXPresso: Project > Properties > C/C++ Build > Settings > Tool Settings > MCU C Compiler > Symbols
    #error Add for example the define "APP_BUILD_TIMESTAMP=$(shell date --utc +%s)" (including quotes)
    #error Add this define to all build configurations.
#endif
/**
 * The define @c APP_BUILD_TIMESTAMP is re-calculated for each C file being compiled, and LPCXPresso caches
 * this value. Building from scratch can give different values per C file, and re-building without cleaning will give
 * the same value (still possibly different per C file).
 * @warning Do not access @c APP_BUILD_TIMESTAMP from any other location. Use @c sArmBuidTimestamp instead.
 */
static uint32_t sArmBuildTimestamp = APP_BUILD_TIMESTAMP;

__attribute__ ((section(".noinit"))) __attribute__((aligned (4)))
static ALON_T sAlon;

__attribute__ ((section(".noinit"))) __attribute__((aligned (4)))
static volatile MEMORY_CONFIG_T sConfig;

/* ------------------------------------------------------------------------- */

/**
 * Dummy variable to test the value of #MEMORY_FIRSTUNUSEDEEPROMOFFSET.
 * If the macro is not correct, the dummy variable will have a negative array size and the compiler will raise an error
 * similar to:
 *   ../src/memory.c:71:13: error: size of array 'sTestLastusedoffset' is negative
 */
static char sTestLastusedoffset[(EEPROM_OFFSET_END == MEMORY_FIRSTUNUSEDEEPROMOFFSET) - 1] __attribute__((unused));

/**
 * Dummy variables to test whether the enum #EVENT_TAG_T is in sync with #APP_MSG_EVENT_T.
 * If not in sync, one of the dummy variables will have a negative array size and the compiler will raise an error
 * similar to:
 *   ../src/memory.c:71:13: error: size of array 'sTestValuesOfEvent.' is negative
 * @{
 */
static char sTestValuesOfEventA[((1 << EVENT_TAG_PRISTINE) == APP_MSG_EVENT_PRISTINE) - 1] __attribute__((unused));
static char sTestValuesOfEventB[((1 << EVENT_TAG_CONFIGURED) == APP_MSG_EVENT_CONFIGURED) - 1] __attribute__((unused));
static char sTestValuesOfEventC[((1 << EVENT_TAG_STARTING) == APP_MSG_EVENT_STARTING) - 1] __attribute__((unused));
static char sTestValuesOfEventD[((1 << EVENT_TAG_LOGGING) == APP_MSG_EVENT_LOGGING) - 1] __attribute__((unused));
static char sTestValuesOfEventE[((1 << EVENT_TAG_STOPPED) == APP_MSG_EVENT_STOPPED) - 1] __attribute__((unused));
static char sTestValuesOfEventF[((1 << EVENT_TAG_TEMPERATURE_TOO_HIGH) == APP_MSG_EVENT_TEMPERATURE_TOO_HIGH) - 1] __attribute__((unused));
static char sTestValuesOfEventG[((1 << EVENT_TAG_TEMPERATURE_TOO_LOW) == APP_MSG_EVENT_TEMPERATURE_TOO_LOW) - 1] __attribute__((unused));
static char sTestValuesOfEventH[((1 << EVENT_TAG_BOD) == APP_MSG_EVENT_BOD) - 1] __attribute__((unused));
static char sTestValuesOfEventI[((1 << EVENT_TAG_FULL) == APP_MSG_EVENT_FULL) - 1] __attribute__((unused));
static char sTestValuesOfEventJ[((1 << EVENT_TAG_EXPIRED) == APP_MSG_EVENT_EXPIRED) - 1] __attribute__((unused));
static char sTestValuesOfEventK[((1 << EVENT_TAG_I2C_ERROR) == APP_MSG_EVENT_I2C_ERROR) - 1] __attribute__((unused));
static char sTestValuesOfEventL[((1 << EVENT_TAG_SPI_ERROR) == APP_MSG_EVENT_SPI_ERROR) - 1] __attribute__((unused));
static char sTestValuesOfEventM[((1 << EVENT_TAG_SHOCK) == APP_MSG_EVENT_SHOCK) - 1] __attribute__((unused));
static char sTestValuesOfEventN[((1 << EVENT_TAG_SHAKE) == APP_MSG_EVENT_SHAKE) - 1] __attribute__((unused));
static char sTestValuesOfEventO[((1 << EVENT_TAG_VIBRATION) == APP_MSG_EVENT_VIBRATION) - 1] __attribute__((unused));
static char sTestValuesOfEventP[((1 << EVENT_TAG_TILT) == APP_MSG_EVENT_TILT) - 1] __attribute__((unused));
static char sTestValuesOfEventQ[((1 << EVENT_TAG_SHOCK_CONFIGURED) == APP_MSG_EVENT_SHOCK_CONFIGURED) - 1] __attribute__((unused));
static char sTestValuesOfEventR[((1 << EVENT_TAG_SHAKE_CONFIGURED) == APP_MSG_EVENT_SHAKE_CONFIGURED) - 1] __attribute__((unused));
static char sTestValuesOfEventS[((1 << EVENT_TAG_VIBRATION_CONFIGURED) == APP_MSG_EVENT_VIBRATION_CONFIGURED) - 1] __attribute__((unused));
static char sTestValuesOfEventT[((1 << EVENT_TAG_TILT_CONFIGURED) == APP_MSG_EVENT_TILT_CONFIGURED) - 1] __attribute__((unused));
static char sTestValuesOfEventQ[((int)EVENT_TAG_COUNT == APP_MSG_EVENT_COUNT) - 1] __attribute__((unused));
/** @} */

/* ------------------------------------------------------------------------- */

/**
 * Used as the default callback function when retrieving events. Use this when extra data is not requested, and only
 * the timestamp of the very first event needs to be retained.
 * @see pEvent_Cb_t
 * @param tag Ignored.
 * @param offset Ignored.
 * @param len Ignored.
 * @param index Ignored.
 * @param timestamp The time when the event was stored.
 * @param context Used as the address where to copy the timestamp to.
 * @return true when @c index equals #EVENT_CB_OPENING_INDEX or #EVENT_CB_CLOSING_INDEX; @c false otherwise.
 */
bool App_EventCb(uint8_t tag, int offset, uint8_t len, unsigned int index, uint32_t timestamp, uint32_t context)
{
    (void)tag; /* suppress [-Wunused-parameter]: in its most simple usage, this argument is ignored. */
    (void)offset; /* suppress [-Wunused-parameter]: in its most simple usage, this argument is ignored. */
    (void)len; /* suppress [-Wunused-parameter]: in its most simple usage, this argument is ignored. */
    (void)index; /* suppress [-Wunused-parameter]: in its most simple usage, this argument is ignored. */
    bool extraCall = (index == EVENT_CB_OPENING_INDEX) || (index == EVENT_CB_CLOSING_INDEX);
    if (!extraCall) {
        uint32_t * pTimestamp = (uint32_t *)context;
        *pTimestamp = timestamp;
    }
    return extraCall; /* Return false after a first proper event has been reported, true otherwise. */
}

/* ------------------------------------------------------------------------- */

bool Memory_Init(void)
{
    bool accepted;
    APP_MSG_EVENT_T events = APP_MSG_EVENT_PRISTINE;

    uint32_t eepromBuildTimestamp;
    Chip_EEPROM_Read(NSS_EEPROM, EEPROM_OFFSET_BUILDTIMESTAMP, &eepromBuildTimestamp, sizeof(uint32_t));
    if (eepromBuildTimestamp != sArmBuildTimestamp) {
        Chip_EEPROM_Write(NSS_EEPROM, EEPROM_OFFSET_BUILDTIMESTAMP, &sArmBuildTimestamp, sizeof(uint32_t));
        Chip_EEPROM_Memset(NSS_EEPROM, EEPROM_OFFSET_CONFIG, 0, sizeof(MEMORY_CONFIG_T));
        Storage_Init();
        /* No need to check and erase the FLASH; when a new configuration is given, storage is reset again with true
         * as argument in SetConfigHandler.
         */
        Storage_Reset(false);
        Memory_ResetConfig(NULL); /* Also calls Event_Init(true); */
        sAlon.uninterrupted = false;
        accepted = false;
    }
    else {
        accepted = true;
        Chip_EEPROM_Read(NSS_EEPROM, EEPROM_OFFSET_CONFIG, (void *)&sConfig, sizeof(MEMORY_CONFIG_T));
        Storage_Init();
        Event_Init(false);
        Chip_PMU_GetRetainedData((uint32_t *)&sAlon, 0, ALON_WORD_SIZE);
        if (Memory_IsMonitoring() && !sAlon.uninterrupted) {
            events |= APP_MSG_EVENT_STOPPED | APP_MSG_EVENT_BOD;
        }
    }

    Validate_Init((uint32_t *)&sConfig.validation);
    Memory_AddToState(events, true); /* May only be called after Event_Init and Validate_Init */
    return accepted;
}

void Memory_DeInit(void)
{
    Validate_DeInit();
    Storage_DeInit();
    Event_DeInit();

    MEMORY_CONFIG_T storedConfig;
    Chip_EEPROM_Read(NSS_EEPROM, EEPROM_OFFSET_CONFIG, &storedConfig, sizeof(MEMORY_CONFIG_T));
    if (memcmp((void *)&sConfig, &storedConfig, sizeof(MEMORY_CONFIG_T))) {
        Chip_EEPROM_Write(NSS_EEPROM, EEPROM_OFFSET_CONFIG, (void *)&sConfig, sizeof(MEMORY_CONFIG_T));
    }
    Chip_PMU_SetRetainedData((uint32_t *)&sAlon, 0, ALON_WORD_SIZE);
}

const MEMORY_CONFIG_T * Memory_GetConfig(void)
{
    return (const MEMORY_CONFIG_T *)&sConfig;
}

bool Memory_IsReadyToStart(void)
{
    bool isReadyToStart = (sConfig.status & (APP_MSG_EVENT_CONFIGURED
            | APP_MSG_EVENT_STARTING | APP_MSG_EVENT_LOGGING | APP_MSG_EVENT_STOPPED)) == APP_MSG_EVENT_CONFIGURED;
    if (isReadyToStart) {
        ASSERT(sConfig.cmd.interval > 0);
    }
    return isReadyToStart;
}

bool Memory_IsMonitoring(void)
{
    bool isMonitoring = (((sConfig.status & (APP_MSG_EVENT_STARTING | APP_MSG_EVENT_LOGGING)) != 0)
            && ((sConfig.status & APP_MSG_EVENT_STOPPED) == 0));
    if (isMonitoring) {
        ASSERT(sConfig.cmd.interval > 0);
    }
    return isMonitoring;
}

bool Memory_IsFull(void)
{
    return (sConfig.status & APP_MSG_EVENT_FULL) != 0;
}

bool Memory_BodOccurred(void)
{
    return (sConfig.status & APP_MSG_EVENT_BOD) != 0;
}

void Memory_ResetConfig(const APP_MSG_CMD_SETCONFIG_T * pCmd)
{
    if (pCmd == NULL) {
        memset((void *)&sConfig, 0, sizeof(MEMORY_CONFIG_T));
    }
    else {
        sConfig.cmd = *pCmd;
        sConfig.status = 0;
        sConfig.validation = 0;
    }
    sConfig.attainedMinimum = 32767; /* reset value */
    sConfig.attainedMaximum = -32768; /* reset value */

    Event_Init(true);
    Memory_AddToState(APP_MSG_EVENT_PRISTINE, true);
}

void Memory_SetAttainedValue(int16_t value)
{
    if (value < sConfig.attainedMinimum) {
        sConfig.attainedMinimum = value;
    }
    if (value > sConfig.attainedMaximum) {
        sConfig.attainedMaximum = value;
    }
}

void Memory_AddToState(uint32_t events, bool ignoreWhenSet)
{
    uint32_t newEvents = events & (~sConfig.status);
    if ((newEvents & (APP_MSG_EVENT_CONFIGURED | APP_MSG_EVENT_STARTING | APP_MSG_EVENT_LOGGING)) != 0) {
        sAlon.uninterrupted = true;
    }

    /* Store all new events separately. */
    EVENT_TAG_T tag = EVENT_TAG_FIRST;
    uint32_t walker = ignoreWhenSet ? newEvents : events;
    while (walker) {
        if (walker & 1) {
            if ((tag == EVENT_TAG_TEMPERATURE_TOO_LOW) || (tag == EVENT_TAG_TEMPERATURE_TOO_HIGH)) {
                uint32_t end = 0; /* It is yet unknown when the excursion ends. This will be updated later. */
                bool success = Event_Set(tag, &end, sizeof(uint32_t));
                ASSERT(success);
                (void)success; /* suppress [-Wunused-parameter]: its value is only asserted in debug builds. */
            }
            else {
                bool success = Event_Set(tag, NULL, 0);
                ASSERT(success);
                (void)success; /* suppress [-Wunused-parameter]: its value is only asserted in debug builds. */
            }
        }
        tag++;
        walker >>= 1;
    }

    sConfig.status |= newEvents;
    /* Call validation code last, after storing in event module and after updating status, so it can rely on that. */
    Validate_NewEvents(newEvents);
}

void Memory_RemoveFromState(uint32_t events)
{
    sConfig.status &= ~events;
}
