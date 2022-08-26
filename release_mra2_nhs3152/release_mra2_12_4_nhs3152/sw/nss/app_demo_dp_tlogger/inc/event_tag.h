/*
 * Copyright 2017-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __EVENT_TAG_H_
#define __EVENT_TAG_H_

/**
 * @addtogroup APP_DEMO_TLOGGER_EVENT Event Tags
 * @ingroup APP_DEMO_TLOGGER
 *  Event Tags defines the different tags that are used to feed the Event Manager with. *
 *  @{
 */

/**
 * Enumeration type describing the different state changes and failures.
 * @pre Must be the unmasked variant of #APP_MSG_EVENT_T
 */
typedef enum EVENT_TAG {
    EVENT_TAG_FIRST = 0,

    EVENT_TAG_FIRST_STATE = EVENT_TAG_FIRST,
    EVENT_TAG_PRISTINE = EVENT_TAG_FIRST_STATE, /**< Unmasked variant of #APP_MSG_EVENT_PRISTINE */
    EVENT_TAG_CONFIGURED, /**< Unmasked variant of #APP_MSG_EVENT_CONFIGURED */
    EVENT_TAG_STARTING, /**< Unmasked variant of #APP_MSG_EVENT_STARTING */
    EVENT_TAG_LOGGING, /**< Unmasked variant of #APP_MSG_EVENT_LOGGING */
    EVENT_TAG_STOPPED, /**< Unmasked variant of #APP_MSG_EVENT_STOPPED */
    EVENT_TAG_LAST_STATE = EVENT_TAG_STOPPED,

    EVENT_TAG_FIRST_FAILURE,
    EVENT_TAG_TEMPERATURE_TOO_HIGH = EVENT_TAG_FIRST_FAILURE, /**< Unmasked variant of #APP_MSG_EVENT_TEMPERATURE_TOO_HIGH */
    EVENT_TAG_TEMPERATURE_TOO_LOW, /**< Unmasked variant of #APP_MSG_EVENT_TEMPERATURE_TOO_LOW */
    EVENT_TAG_BOD, /**< Unmasked variant of #APP_MSG_EVENT_BOD */
    EVENT_TAG_FULL, /**< Unmasked variant of #APP_MSG_EVENT_FULL */
    EVENT_TAG_EXPIRED, /**< Unmasked variant of #APP_MSG_EVENT_EXPIRED */
    EVENT_TAG_I2C_ERROR, /**< Unmasked variant of #APP_MSG_EVENT_I2C_ERROR */
    EVENT_TAG_SPI_ERROR, /**< Unmasked variant of #APP_MSG_EVENT_SPI_ERROR */
    EVENT_TAG_LAST_FAILURE = EVENT_TAG_SPI_ERROR,

    EVENT_TAG_FIRST_ACCELERATION,
    EVENT_TAG_SHOCK = EVENT_TAG_FIRST_ACCELERATION, /**< Unmasked variant of #APP_MSG_EVENT_SHOCK */
    EVENT_TAG_SHAKE, /**< Unmasked variant of #APP_MSG_EVENT_SHAKE */
    EVENT_TAG_VIBRATION, /**< Unmasked variant of #APP_MSG_EVENT_VIBRATION */
    EVENT_TAG_TILT, /**< Unmasked variant of #APP_MSG_EVENT_TILT */
    EVENT_TAG_SHOCK_CONFIGURED, /**< Unmasked variant of #APP_MSG_EVENT_SHOCK_CONFIGURED */
    EVENT_TAG_SHAKE_CONFIGURED, /**< Unmasked variant of #APP_MSG_EVENT_SHAKE_CONFIGURED */
    EVENT_TAG_VIBRATION_CONFIGURED, /**< Unmasked variant of #APP_MSG_EVENT_VIBRATION_CONFIGURED */
    EVENT_TAG_TILT_CONFIGURED, /**< Unmasked variant of #APP_MSG_EVENT_TILT_CONFIGURED */
    EVENT_TAG_LAST_ACCELERATION = EVENT_TAG_TILT_CONFIGURED,

    EVENT_TAG_FIRST_HUMIDITY,
    EVENT_TAG_HUMIDITY_CONFIGURED = EVENT_TAG_FIRST_HUMIDITY, /**< Unmasked variant of #APP_MSG_EVENT_HUMIDITY_CONFIGURED */
    EVENT_TAG_HUMIDITY_TOO_HIGH, /**< Unmasked variant of #APP_MSG_EVENT_HUMIDITY_TOO_HIGH */
    EVENT_TAG_HUMIDITY_TOO_LOW, /**< Unmasked variant of #APP_MSG_EVENT_HUMIDITY_TOO_LOW */
    EVENT_TAG_LAST_HUMIDITY = EVENT_TAG_HUMIDITY_TOO_LOW,

    EVENT_TAG_LAST = EVENT_TAG_LAST_HUMIDITY,

    /** Number of possible events. Not to be used as a possible event. Use this in for loops or to define array sizes. */
    EVENT_TAG_COUNT
} EVENT_TAG_T;

#endif /** @} */
