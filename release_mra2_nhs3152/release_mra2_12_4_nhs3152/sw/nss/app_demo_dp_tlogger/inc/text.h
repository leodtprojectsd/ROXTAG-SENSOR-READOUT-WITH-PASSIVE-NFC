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

#ifndef __TEXT_H_
#define __TEXT_H_

/** @addtogroup APP_DEMO_TLOGGER_TEXT Stringifier
 * @ingroup APP_DEMO_TLOGGER
 * The text functions provide the ability to the Temperature Logger to translate its current status in text form. This
 * allows to create NDEF TEXT records conveying the current state, which in turn allows to demonstrate the NHS31xx
 * capabilities to create NDEF standard messages since they can be parsed and displayed by an NFC-enabled Android phone
 * without the need of a dedicated APP; or by any recent iPhone running a generic NFC APP - such NXP's Tag Info.
 * The text can also be copied and presented as is in any dedicated APP running on the tag reader.
 *
 * Language is American English.
 *
 * @{
 */

/** The maximum size of the buffer returned by #Text_GetStatus */
#define TEXT_MAX_STATUS_LENGTH 46

/** The maximum size of one stringified failure. */
#define TEXT_MAX_FAILURE_LENGTH 46

/** The maximum size of the buffer returned by #Text_GetFailures */
#define TEXT_MAX_FAILURES_LENGTH (7 * TEXT_MAX_FAILURE_LENGTH)

/** The maximum size of the buffer returned by #Text_GetTemperature */
#define TEXT_MAX_TEMPERATURE_LENGTH 59

/* ------------------------------------------------------------------------- */

/**
 * Retrieve a textual representation of the current status.
 * @param [out] pLen : May not be @c NULL. The string length in bytes will be written here when the function returns.
 * @return A pointer to a string of @c *pLen characters, less than or equal to #TEXT_MAX_STATUS_LENGTH
 * @note The data pointed to remains valid until SRAM is cleared.
 * @note Accesses the memory application file (memory.c/h), storage module and event module.
 */
const char * Text_GetStatus(int * pLen);

/**
 * Retrieve a textual representation of the current failures.
 * @param [out] pLen : May not be @c NULL. The string length in bytes will be written here when the function returns.
 * @return A pointer to a string of @c *pLen characters, less than or equal to
 *  (@c EVENT_TAG_LAST_FAILURE - @c EVENT_TAG_FIRST_FAILURE + 1) * #TEXT_MAX_FAILURE_LENGTH
 * @note The data pointed to remains valid until SRAM is cleared.
 * @note Accesses the memory application file (memory.c/h) and event module.
 */
const char * Text_GetFailures(int * pLen);

/**
 * Retrieve a textual representation of the current temperature.
 * @param [out] pLen : May not be @c NULL. The string length in bytes will be written here when the function returns.
 * @return A pointer to a string of @c *pLen characters, less than or equal to #TEXT_MAX_TEMPERATURE_LENGTH
 * @note The data pointed to remains valid until SRAM is cleared.
 * @note Accesses the memory application file (memory.c/h).
 */
const char * Text_GetTemperature(int * pLen);

#endif /** @} */
