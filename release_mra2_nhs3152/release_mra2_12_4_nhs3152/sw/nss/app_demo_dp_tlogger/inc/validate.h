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

#ifndef __VALIDATE_H_
#define __VALIDATE_H_

/** @defgroup APP_DEMO_TLOGGER_VALIDATE Measurement and event validation
 * @ingroup APP_DEMO_TLOGGER
 * The validate functions provide a generic interface via which a highly use-case specific validation algorithm can
 * determine the current state of the logging and monitoring process.
 *
 * @note The validation has a close relationship with @ref APP_DEMO_TLOGGER_MEMORY, as they will both call each other's
 *  API. Be careful with the order of initialization and calling the API's of the two blocks of code.
 * @{
 */

#include <stdint.h>
#include <stdbool.h>

/* -------------------------------------------------------------------------------- */

/**
 * Initializes file scoped variables.
 * @param pWorkspace Points to a 32-bit value free to be used by the validation process. Any change to this value is
 *  assumed to be stored in EEPROM or ALON before going to Power-off or Deep Power Down mode and given back unaltered
 *  in a next call to this function. This allows the validation process to keep track of it own internal state.
 * @note The value pointed to given may not change too often, never hitting the official endurance limit of 10k.
 * @pre Must be the first function called in this file.
 * @pre Must be called before changes to state and/or configuration are to be made.
 * @pre All calls to this function @b must provide the same workspace.
 * @warning pWorkspace If @c NULL is given, functionality may be severely limited. Check the implementation for details.
 */
void Validate_Init(uint32_t * pWorkspace);

/**
 * De-initializes the validation process.
 * @warning This call can still change the contents pointed to by @c pWorkspace (see #Validate_Init).
 * @warning This call can still change #MEMORY_CONFIG_T.status.
 */
void Validate_DeInit(void);

/**
 * To be called once @b per active lifetime: when monitoring - of whatever - starts, this call ensures the validation
 * starts with a good known state, using initial values.
 * @note Do @b not call this function each time after starting up or waking up from a low power state, as it will reset
 *  all intermediate validation data and conclusions.
 * @warning This call may take a long time to complete. See the specific implementation for details.
 */
void Validate_Reset(void);

/**
 * Checks the data from #Memory_GetConfig and takes proper actions based on the updated data.
 * @pre #Memory_Init must have been called.
 * @param temperature The last temperature that was measured.
 * @warning This call may take a long time to complete. See the specific implementation for details.
 */
void Validate_Temperature(int16_t temperature);

/**
 * For each new event, call this function to ensure the algorithm takes them properly into account.
 * @pre #Memory_Init must have been called.
 * @param newEvents A bitmask of OR'd events of type #EVENT_TAG_T. Only new events, which have not yet been reported
 *  are present.
 * @warning This call may take a long time to complete. See the specific implementation for details.
 * @pre All new events have already been handled by the memory application file, and can be queried for in the event
 *  handler module.
 */
void Validate_NewEvents(uint32_t newEvents);

#endif /** @} */
