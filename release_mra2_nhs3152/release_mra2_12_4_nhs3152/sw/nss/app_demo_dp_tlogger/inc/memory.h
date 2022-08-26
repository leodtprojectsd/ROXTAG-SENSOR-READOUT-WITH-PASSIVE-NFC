/*
 * Copyright 2016-2018 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __MEMORY_H_
#define __MEMORY_H_

/**
 * @addtogroup APP_DEMO_TLOGGER_MEMORY Memory block
 * @ingroup APP_DEMO_TLOGGER
 *  The Temperature Logger Demo Memory block gathers all accesses to data that must persist across active sessions.
 *  It will group access to:
 *  - @ref MODS_NSS_STORAGE
 *  - @ref MODS_NSS_EVENT
 *  - EEPROM not in use by the above modules
 *  - GPREG not in use by the above modules
 *
 *  It will maintain the current configuration, allowing access to the different parameters and states in SRAM, and
 *  ensuring the data will persist when going to Deep Power Down or Power-off states.
 *  To this end, it adds API to update the current state and convenience functions to quickly check for specific states.
 *
 *  @par Dependencies
 *  - There is a coupling with the events module: updating the current state through #Memory_AddToState will result in
 *      calls to #Event_Set whenever new states are entered.
 *  - There is a coupling with the storage module: when it is detected during #Memory_Init the firmware has changed,
 *      #Storage_Reset will be called to wipe all gathered data.
 *  - There is a dependency with the validate block: the workspace memory is maintained by this block - see
 *      #MEMORY_CONFIG_T.validation.
 *  .
 *  Due to these dependencies, the memory block will also initialize respectively de-initialize the above mentioned
 *  modules/block by calling #Validate_Init, #Storage_Init and #Event_Init; respectively #Validate_DeInit,
 *  #Storage_DeInit and #Event_DeInit.
 *
 *  Apart of the above, calls to the mentioned modules/block still need to be carried out by the application.
 *
 *  @{
 */

#include <stdint.h>
#include <stdbool.h>
#include "msghandler_protocol.h"

/* ------------------------------------------------------------------------- */

/**
 * The offset in EEPROM memory to the first unclaimed byte by this storage component.
 * The EEPROM region from the very first byte up to (not including) this offset is under full control of this component
 * and no other code is assumed to use it.
 */
#define MEMORY_FIRSTUNUSEDEEPROMOFFSET 36

/**
 * Defines the constant configuration under which to operate;
 * Also stores some data of the ongoing monitoring and logging session
 * @warning be sure to only list fields here that are updated very sparingly: each time these contents are changed,
 *  they will be written to EEPROM in #Memory_DeInit. EEPROM has a guaranteed endurance of 10000 writes (per page) only.
 */
typedef struct MEMORY_CONFIG_S {
    APP_MSG_CMD_SETCONFIG_T cmd; /**< As given by the tag reader. */
    int16_t attainedMinimum; /**< The absolute minimum value recorded in deci-Celsius degrees. */
    int16_t attainedMaximum; /**< The absolute maximum value recorded in deci-Celsius degrees. */
    /* count - number of stored samples - is maintained by storage module. */
    uint32_t status; /**< An OR'd combination of #APP_MSG_EVENT_T events that reflect the current state. */
    /* startTime - time of first measurement is maintained by event module. */
    /* currentTime is is maintained by the RTC HW block. */
    uint32_t validation; /**< A value solely read from and written to by validate.c file */
} MEMORY_CONFIG_T;

/* ------------------------------------------------------------------------- */

/**
 * Initialization function. Must be called first in this component.
 * @note Also calls #Storage_Init, #Event_Init, #Validate_Init
 * @pre Must be the first function called in this file.
 * @pre #Chip_EEPROM_Init has been called beforehand
 * @return Whether the contents available in NVM were accepted. After flashing another image the contents are no longer
 *  accepted and a blank slate is used. In that case, @c false is returned. When @c true is returned, the contents
 *  that were set in NVM were read out and used.
 */
bool Memory_Init(void);

/**
 * De-Initializes the component.
 * @note Also calls #Storage_DeInit(), #Event_DeInit, #Validate_DeInit.
 * @pre Must be called before going to Power-off or Deep Power Down mode.
 * @post Must be called last in this file.
 */
void Memory_DeInit(void);

/* ------------------------------------------------------------------------- */

/**
 * Retrieve configuration.
 * @return A pointer to the filled-in MEMORY_CONFIG_T structure.
 * @note Multiple calls return the same pointer.
 */
const MEMORY_CONFIG_T * Memory_GetConfig(void);

/**
 * Helper function. Checks #MEMORY_CONFIG_T.status solely.
 * @return @c true if a new measurement will be made after some delay. There may be no measurements made yet.
 */
bool Memory_IsMonitoring(void);

/**
 * Helper function. Checks #MEMORY_CONFIG_T.status solely.
 * @return @c true if storage is full - based on the previous calls to #Memory_AddToState - and no new measurement can
 *  be stored.
 */
bool Memory_IsFull(void);

/**
 * Helper function. Checks #MEMORY_CONFIG_T.status solely.
 * @return @c true if a low battery voltage has been detected - using the bod HW block.
 */
bool Memory_BodOccurred(void);

/**
 * Helper function. Checks #MEMORY_CONFIG_T.status solely.
 * @return @c true if a configuration is present but no measurement is available, nor is due.
 */
bool Memory_IsReadyToStart(void);

/**
 * Resets all parameters.
 * @param pCmd : pointer to a structure where all new parameters are to be copied from. May be @c NULL.
 *  If equal to @c NULL, all values are reset to default values.
 * @note Regardless of the given argument, the fields #MEMORY_CONFIG_T.attainedMinimum,
 *  #MEMORY_CONFIG_T.attainedMaximum and #MEMORY_CONFIG_T.status will explicitly be set to
 *  default values - so no need to be filled in by the caller.
 */
void Memory_ResetConfig(const APP_MSG_CMD_SETCONFIG_T * pCmd);

/**
 * Updates the attained extremities of the measured values.
 * There is @b no update of #MEMORY_CONFIG_T.status - Use #Memory_AddToState.
 * @param value The new value to compare against the current extremities.
 * @note The change is made to the memory pointed to by #Memory_GetConfig.
 * @note Changes made are saved in NVM during #Memory_DeInit.
 */
void Memory_SetAttainedValue(int16_t value);

/**
 * Updates #MEMORY_CONFIG_T.status by adding bits to the bitmask.
 * @note Every bit added will cause a new entry in @ref MODS_NSS_EVENT
 * @note @ref APP_DEMO_TLOGGER_VALIDATE is notified for each set bit through #Validate_NewEvents.
 * @param events An OR'd combination of #APP_MSG_EVENT_T events that must be added.
 * @param ignoreWhenSet
 *  - If @c true, the bits that are already set in #MEMORY_CONFIG_T.status are ignored.
 *  - If @c false, every bit set will result in a new entry in @ref MODS_NSS_EVENT, and will be passed on for
 *      validation.
 *  .
 */
void Memory_AddToState(uint32_t events, bool ignoreWhenSet);

/**
 * Updates #MEMORY_CONFIG_T.status by removing bits from the bitmask.
 * @note Entries in @ref MODS_NSS_EVENT are left untouched.
 * @param events An OR'd combination of #APP_MSG_EVENT_T events that must be removed. The bits that are not set in
 *  #MEMORY_CONFIG_T.status are ignored.
 */
void Memory_RemoveFromState(uint32_t events);

/** @} */
#endif
