/*
 * Copyright 2018-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __TEMPERATURE_H_
#define __TEMPERATURE_H_

/** @addtogroup APP_DEMO_TLOGGER_TEMPERATURE Temperature block
 * @ingroup APP_DEMO_TLOGGER
 * The Temperature block shields the @ref MODS_NSS_TMEAS module from the rest of the application. it also implements
 * the callback #TMEAS_CB as set in @c app_sel.h
 * @{
 */

#include "board.h"

/* ------------------------------------------------------------------------- */

/**
 * Sets the cached value to #APP_MSG_TEMPERATURE_PLACEHOLDER_VALUE.
 */
void Temperature_Reset(void);

/**
 * Wrapper around #TMeas_Measure.
 * @param resolution See #TMeas_Measure
 * @param requestedExternally
 *  - If @c true, the measured value will be given to @ref MODS_NSS_MSG via #Msg_AddResponse as a
 *      #APP_MSG_ID_MEASURETEMPERATURE response, @b and cached internally, so it can be fetched using #Temperature_Get.
 *  - else: the value is only stored.
 * @return See #TMeas_Measure
 */
int Temperature_Measure(TSEN_RESOLUTION_T resolution, bool requestedExternally);

/**
 * - If a temperature measurement is ongoing: waits until the conversion is ready, and returns that last measured value.
 * - Else: returns the last cached value: either a temperature value, or #APP_MSG_TEMPERATURE_PLACEHOLDER_VALUE
 * .
 * @pre #Temperature_Measure has been called at least once.
 * @returns the last measured temperature. Expressed in deci-celsius.
 */
int Temperature_Get(void);

#endif /** @} */
