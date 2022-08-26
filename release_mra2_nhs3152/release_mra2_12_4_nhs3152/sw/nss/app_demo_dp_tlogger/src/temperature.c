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

#include <stddef.h>
#include "board.h"
#include "msg/msg.h"
#include "tmeas/tmeas.h"
#include "msghandler_protocol.h"

/* ------------------------------------------------------------------------- */

void App_TmeasCb(TSEN_RESOLUTION_T resolution, TMEAS_FORMAT_T format, int value, uint32_t context);
static void SendMeasureTemperatureResponse(bool success, int16_t temperature);

/**
 * The last temperature measurement value, expressed in deci-Celsius. To be stored in non-volatile memory or
 * used for quick status reports.
 * @see Temperature_Get
 */
static volatile int sLastMeasurement = APP_MSG_TEMPERATURE_PLACEHOLDER_VALUE;

/* ------------------------------------------------------------------------- */

/**
 * Called under interrupt.
 * @see TMEAS_CB
 * @see pTMeas_Cb_t
 */
void App_TmeasCb(TSEN_RESOLUTION_T resolution, TMEAS_FORMAT_T format, int value, uint32_t context)
{
    (void)resolution; /* suppress [-Wunused-parameter]: we don't care; we just accept the value. This argument is ignored. */
    (void)format; /* suppress [-Wunused-parameter]: only TMEAS_FORMAT_CELSIUS is used. This argument's value is assumed. */
    ASSERT(format == TMEAS_FORMAT_CELSIUS);
    if (value < APP_MSG_MIN_TEMPERATURE) {
        value = APP_MSG_MIN_TEMPERATURE;
    }
    if (APP_MSG_MAX_TEMPERATURE < value) {
        value = APP_MSG_MAX_TEMPERATURE;
    }

    sLastMeasurement = value;

    /* Based on the context, we deduce the reason of measuring the temperature. This context is given as last argument
     * in a call to TMeas_Measure()
     */
    if (context) {
        /* A live measurement was requested while communicating. Wrap it in a response, and send it off. */
        /* resolution == can vary */
        SendMeasureTemperatureResponse(value != TMEAS_ERROR, (int16_t)value);
    }
    /* else:
     * - Either a measurement was requested while not communicating. This value is to be stored internally, until a tag
     *  reader makes an NFC connection and starts reading out all the stored samples. resolution == TSEN_10BITS
     * - or this value will be used in the initial response. resolution == TSEN_7BITS
     */
}

/**
 * @note It is expected this function only to be called as the last step in the process initiated by sending a
 *  #APP_MSG_CMD_MEASURETEMPERATURE_T command.
 * @param success The command result. Only when @c success equals @c true, the contents of @c temperature must be valid.
 * @param temperature The measured temperature in deci-Celsius degrees
 */
static void SendMeasureTemperatureResponse(bool success, int16_t temperature)
{
    APP_MSG_RESPONSE_MEASURETEMPERATURE_T response;
    response.result = success ? MSG_OK : APP_MSG_ERR_TSEN;
    response.temperature = temperature;
    Msg_AddResponse(APP_MSG_ID_MEASURETEMPERATURE, sizeof(response), (uint8_t*)&response);
}

/* ------------------------------------------------------------------------- */

void Temperature_Reset(void)
{
    sLastMeasurement = APP_MSG_TEMPERATURE_PLACEHOLDER_VALUE;
}

int Temperature_Measure(TSEN_RESOLUTION_T resolution, bool requestedByHost)
{
    return TMeas_Measure(resolution, TMEAS_FORMAT_CELSIUS, false, requestedByHost /* Value used in App_TmeasCb */);
}

int Temperature_Get(void)
{
    while (Chip_TSen_ReadStatus(NSS_TSEN, NULL) & TSEN_STATUS_SENSOR_IN_OPERATION) {
        ; /* Wait until the temperature has become available in sLastMeasurement. */
    }
    return sLastMeasurement;
}
