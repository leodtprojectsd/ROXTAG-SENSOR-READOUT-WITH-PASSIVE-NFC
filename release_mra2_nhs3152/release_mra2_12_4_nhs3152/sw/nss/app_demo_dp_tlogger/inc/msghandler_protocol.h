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

#ifndef __MSGHANDLER_PROTOCOL_H_
#define __MSGHANDLER_PROTOCOL_H_

/**
 * @addtogroup APP_DEMO_TLOGGER_MSGHANDLER_PROTOCOL 'tlogger' app.spec. messages
 * @ingroup APP_DEMO_TLOGGER_MSGHANDLER 
 *  This file describes the application specific commands and responses used in the Temperature Logger demo application.
 * @{
 */

#include "msg/msg.h"

/* -------------------------------------------------------------------------------- */

/**
 * The temperature sensor was already in use. Wait, then try again.
 * The time to wait is dependent on the resolution of the temperature conversion currently in progress - see
 * #APP_MSG_TSEN_RESOLUTION_T - and is at most 100 ms.
 */
#define APP_MSG_ERR_TSEN 0x1000E

/**
 * The maximum temperature the application can handle. This is a result of the limitations of the IC (-40:+85C), the
 * limitations of the battery (say, -30:+50C), and the requirements of the use case.
 * Since this is a @b demo, the maximum temperature the IC validated for is chosen.
 * - All measured temperatures are clamped in the range [MIN-MAX].
 * - Expressed in deci-degrees.
 */
#define APP_MSG_MIN_TEMPERATURE -400

/**
 * The maximum temperature the application can handle. This is a result of the limitations of the IC (-40:+85C), the
 * limitations of the battery (say, -30:+50C), and the requirements of the use case.
 * Since this is a @b demo, the maximum temperature the IC validated for is chosen.
 * - All measured temperatures are clamped in the range [MIN-MAX].
 * - Expressed in deci-degrees.
 */
#define APP_MSG_MAX_TEMPERATURE 850

/**
 * A value used to indicate either
 * - a value above #APP_MSG_MAX_TEMPERATURE or below -#APP_MSG_MAX_TEMPERATURE was measured. The measurement value was
 *  replaced with this value. Or
 * - a measurement was due while an NFC field was present. The measurement did not take place and this value was stored
 * instead.
 */
#define APP_MSG_TEMPERATURE_PLACEHOLDER_VALUE (APP_MSG_MAX_TEMPERATURE + 1)

/**
 * The value used to indicate the first measurement is to be delayed until an explicit command with message id
 * #APP_MSG_ID_START is given. See #APP_MSG_CMD_SETCONFIG_T.startDelay.
 */
#define APP_MSG_DELAY_START_INDEFINITELY 0xFFFFFFFF

/**
 * The maximum number of temperature measurement values that can be retrieved in one response.
 * @see APP_MSG_ID_GETMEASUREMENTS
 * @see APP_MSG_ID_GETPERIODICDATA
 */
#define APP_MSG_MAX_TEMPERATURE_VALUES_IN_RESPONSE 232
#if APP_MSG_MAX_TEMPERATURE_VALUES_IN_RESPONSE > 255
    #error APP_MSG_MAX_TEMPERATURE_VALUES_IN_RESPONSE must fit in one byte for APP_MSG_ID_GETMEASUREMENTS
#endif

/**
 * Helper macro to calculate the size of a single event, appended after a #APP_MSG_CMD_GETEVENTS_T response.
 * @param info : A bitmask of OR'd event information types of type #EVENT_INFO_T.
 * @param data : The number of bytes of the extra data stored with the event. Ignored if #EVENT_INFO_DATA is not set in
 *  @c info.
 */
#define APP_MSG_SIZEOFEVENT_IN_RESPONSE(info, data) \
    (((((info) & EVENT_INFO_INDEX) == EVENT_INFO_INDEX) * 2) \
    + ((((info) & EVENT_INFO_TIMESTAMP) == EVENT_INFO_TIMESTAMP) * 4) \
    + (((info) & EVENT_INFO_ENUM) == EVENT_INFO_ENUM) \
    + ((((info) & EVENT_INFO_DATA) == EVENT_INFO_DATA) * (data)))

/**
 * The maximum number of events that can be retrieved in a one response.
 * @param size : the size of one event, appended after a #APP_MSG_CMD_GETEVENTS_T response. All events are assumed to
 *  be of equal size (i.e. ignoring potential differences when #EVENT_INFO_DATA is set).
 */
#define APP_MSG_MAX_EVENTS_IN_RESPONSE(size) (505 / (size))

/**
 * Supported messages
 */
typedef enum APP_MSG_ID {
    /**
     * @c 0x46 @n
     * Retrieves (part of) the stored measurements, that were taken after the last #APP_MSG_ID_SETCONFIG command.
     * @param APP_MSG_CMD_GETMEASUREMENTS_T
     * @return #MSG_RESPONSE_RESULTONLY_T if the command could not be handled;
     *  #APP_MSG_RESPONSE_GETMEASUREMENTS_T otherwise.
     * @note synchronous command
     * @note All temperatures retrieved are within the range [-#APP_MSG_MAX_TEMPERATURE; +#APP_MSG_MAX_TEMPERATURE].
     *  There is one special value, #APP_MSG_TEMPERATURE_PLACEHOLDER_VALUE, that is used to indicate anomalies.
     */
    APP_MSG_ID_GETMEASUREMENTS = 0x46,

    /**
     * @c 0x48 @n
     * Retrieves all configuration parameters regarding temperature and chip behavior, and the number of temperature
     * measurements available.
     * @param none
     * @return #APP_MSG_RESPONSE_GETCONFIG_T
     * @note synchronous command
     * @note The response of this command will be buffered by the application in addition to being sent out immediately.
     *   It will then also be placed in the same NFC message that contains the response for the
     *   #MSG_ID_GETVERSION command.
     */
    APP_MSG_ID_GETCONFIG = 0x48,

    /**
     * @c 0x49 @n
     * Sets all configuration parameters regarding temperature and chip behavior, and clears the buffer holding all
     * measurements and events.
     * @param APP_MSG_CMD_SETCONFIG_T
     * @return #MSG_RESPONSE_RESULTONLY_T
     * @note synchronous command
     */
    APP_MSG_ID_SETCONFIG = 0x49,

    /**
     * @c 0x50 @n
     * Measures the temperature using the built-in temperature sensor.
     * @param APP_MSG_CMD_MEASURETEMPERATURE_T
     * @return #MSG_RESPONSE_RESULTONLY_T immediately; @n
     *  If @c result was equal to #MSG_OK, #APP_MSG_RESPONSE_MEASURETEMPERATURE_T thereafter. This may take up to
     *  100 ms. This second response must be fetched by issuing a command with #MSG_ID_GETRESPONSE.
     * @note asynchronous command
     */
    APP_MSG_ID_MEASURETEMPERATURE = 0x50,

    /**
     * @c 0x5A @n
     * Starts logging according to the last given configuration using #APP_MSG_ID_SETCONFIG.
     * A first measurement will be taken immediately.
     * @param none
     * @return #MSG_RESPONSE_RESULTONLY_T
     * @note synchronous command
     */
    APP_MSG_ID_START = 0x5A,

    /**
     * @c 0x5B @n
     * Retrieves events logged by the application. Changes in execution state and detection of anomalies are recorded
     * with a full timestamp and are annotations to the logged measurement values.
     * @param #APP_MSG_CMD_GETEVENTS_T
     * @return #APP_MSG_RESPONSE_GETEVENTS_T
     * @note synchronous command
     */
    APP_MSG_ID_GETEVENTS = 0x5B,

    /**
     * @c 0x5E @n
     * Retrieves (part of the) data that was taken periodically. No timing information will be provided, as that can be
     * deduced from the sequence and the configuration details - see #APP_MSG_ID_GETCONFIG.
     * @param APP_MSG_CMD_GETPERIODICDATA_T
     * @return #MSG_RESPONSE_RESULTONLY_T if the command could not be handled;
     *  #APP_MSG_RESPONSE_GETPERIODICDATA_T otherwise.
     * @note synchronous command
     */
    APP_MSG_ID_GETPERIODICDATA = 0x5E,

    /** Number of application specific message IDs. Not to be used as a possible ID. Use this in for loops or to define array sizes. */
    APP_MSG_ID_COUNT = 7
} APP_MSG_ID_T;

/**
 * The different events that can occur and are tracked - used in #APP_MSG_RESPONSE_GETCONFIG_T.status.
 * - All events that indicate a failure or achievement provide information can co-exist.
 * - All events that indicate a state change remain set even when they are superseded with a new state change:
 *  e.g. #APP_MSG_EVENT_PRISTINE will remain set after being configured, indicated by bit @c APP_MSG_EVENT_CONFIGURED.
 * .
 * @note This enumeration is shared across multiple demo firmware applications; not all events listed here may be
 *  applicable.
 * @see APP_MSG_ID_GETCONFIG
 */
typedef enum APP_MSG_EVENT {
    /** @c 0x0000 0001 @n State change: the IC no longer has a configuration and contains no data. */
    APP_MSG_EVENT_PRISTINE = 1 << 0,

    /** @c 0x0000 0002 @n State change: the IC is configured, but requires a #APP_MSG_ID_START command to start. */
    APP_MSG_EVENT_CONFIGURED = 1 << 1,

    /** @c 0x0000 0004 @n State change: the IC is configured and will make a first measurement after a delay. */
    APP_MSG_EVENT_STARTING = 1 << 2,

    /** @c 0x0000 0008 @n State change: the IC is configured and is logging. At least one sample is available. */
    APP_MSG_EVENT_LOGGING = 1 << 3,

    /** @c 0x0000 0010 @n State change: the IC is configured and has been logging. Now it has stopped logging. */
    APP_MSG_EVENT_STOPPED = 1 << 4,

    /** @c 0x0000 0020 @n Failure: at least one temperature was strictly higher than the valid maximum value. */
    APP_MSG_EVENT_TEMPERATURE_TOO_HIGH = 1 << 5,

    /** @c 0x0000 0040 @n Failure: at least one temperature was strictly lower than the valid minimum value. */
    APP_MSG_EVENT_TEMPERATURE_TOO_LOW = 1 << 6,

    /** @c 0x0000 0080 @n Failure: a brown-out is about to occur or has occurred. Battery is (nearly) depleted. */
    APP_MSG_EVENT_BOD = 1 << 7,

    /** @c 0x0000 0100 @n Failure: logging has stopped because no more free space is available to store samples. */
    APP_MSG_EVENT_FULL = 1 << 8,

    /**
     * @c 0x0000 0200 @n
     * Achievement: logging has stopped because the time spent logging has exceeded the non-zero value set in
     * APP_MSG_CMD_SETCONFIG_T.runningTime.
     */
    APP_MSG_EVENT_EXPIRED = 1 << 9,

    /**
     * @c 0x0000 0400 @n
     * Temporary error: The application could not successfully communicate using I2C with an external sensor.
     * Possible causes are
     * - a bad connection: this may indicate problems in the layout and manufacturing of the product.
     * - a power failure: this may indicate problems in the power supply of the external sensor / IC.
     * - a concurrency problem: this may indicate synchronization problems, as the external sensor / IC could not reply.
     * .
     * Depending on environmental conditions this problem is likely to be temporary. When I2C communication is
     * successful next time, this bit is cleared again.
     */
    APP_MSG_EVENT_I2C_ERROR = 1 << 10,

    /**
     * @c 0x0000 0800 @n
     * Temporary error: The application could not successfully communicate using SPI with an external sensor.
     * Possible causes are
     * - a bad connection: this may indicate problems in the layout and manufacturing of the product.
     * - a power failure: this may indicate problems in the power supply of the external sensor / IC.
     * - a concurrency problem: this may indicate synchronization problems, as the external sensor / IC could not reply.
     * .
     * Depending on environmental conditions this problem is likely to be temporary. When SPI communication is
     * successful next time, this bit is cleared again.
     */
    APP_MSG_EVENT_SPI_ERROR = 1 << 11,

    /** @c 0x0000 1000 @n Storage condition notification: the product has received a shock. */
    APP_MSG_EVENT_SHOCK = 1 << 12,

    /** @c 0x0000 2000 @n Storage condition notification: the product has been shaken with. */
    APP_MSG_EVENT_SHAKE = 1 << 13,

    /** @c 0x0000 4000 @n Storage condition notification: the product has detected a vibration. */
    APP_MSG_EVENT_VIBRATION = 1 << 14,

    /** @c 0x0000 8000 @n Storage condition notification: the product has detected a tilt. */
    APP_MSG_EVENT_TILT = 1 << 15,

    /** @c 0x0001 0000 @n Storage condition notification: the product is configured to detect shocks. */
    APP_MSG_EVENT_SHOCK_CONFIGURED = 1 << 16,

    /** @c 0x0002 0000 @n Storage condition notification: the product is configured to detect shaking. */
    APP_MSG_EVENT_SHAKE_CONFIGURED = 1 << 17,

    /** @c 0x0004 0000 @n Storage condition notification: the product is configured to detect vibrations. */
    APP_MSG_EVENT_VIBRATION_CONFIGURED = 1 << 18,

    /** @c 0x0008 0000 @n Storage condition notification: the product is configured to detect tilts. */
    APP_MSG_EVENT_TILT_CONFIGURED = 1 << 19,

    /** @c 0x0010 0000 @n Storage condition notification: the product is configured to detect humidity. */
    APP_MSG_EVENT_HUMIDITY_CONFIGURED = 1 << 20,

    /** @c 0x0020 0000 @n Failure: at least one humidity measurement was strictly higher than the valid maximum value. */
    APP_MSG_EVENT_HUMIDITY_TOO_HIGH = 1 << 21,

    /** @c 0x0040 0000 @n Failure: at least one humidity measurement was strictly lower than the valid minimum value. */
    APP_MSG_EVENT_HUMIDITY_TOO_LOW = 1 << 22,

    /** @c 23 @n Number of possible events. Not to be used as a possible event. Use this in for loops or to define array sizes. */
    APP_MSG_EVENT_COUNT = 23,

    /** Convenience value. Not to be used as a possible event. Use this in bitmasks to capture all events. */
    APP_MSG_EVENT_ALL = (1 << APP_MSG_EVENT_COUNT) - 1
} APP_MSG_EVENT_T;

/**
 * Lists the different types of information that can be returned in a response with message id #APP_MSG_ID_GETEVENTS
 * - the data following the #APP_MSG_RESPONSE_GETCONFIG_T structure.
 */
typedef enum EVENT_INFO {
    /**
     * @c 0x01 @n
     * When set, include the absolute index number of the event.
     * - Size: 2 bytes, LSByte first
     * .
     */
    EVENT_INFO_INDEX = 1 << 0,

    /**
     * @c 0x02 @n
     * When set, include the absolute timestamp when the event occurred.
     * - Size: 4 bytes, LSByte first
     * .
     */
    EVENT_INFO_TIMESTAMP = 1 << 1,

    /**
     * @c 0x04 @n
     * When set, include the event number @c n. This is equal to log_2 of the enumeration value of type #APP_MSG_EVENT_T.
     * The enumeration value can be retrieved by shifting @c 1 @n positions to the left.
     * - Size: 1 byte
     * .
     */
    EVENT_INFO_ENUM = 1 << 2,

    /**
     * @c 0x08 @n
     * When set, include the extra data stored with the event, if any.
     * - Size: variable, depending on the event being stored.
     * .
     * @note The size is @b not passed along. It is assumed the tag reader knows this in advance, as it knows how to
     *  interpret this data.
     */
    EVENT_INFO_DATA = 1 << 3,

    /**
     * @c 4 @n Number of possible types of event information. Not to be used as a possible enumeration value.
     * Use this in for loops or to define array sizes.
     */
    EVENT_INFO_COUNT = 4,

    /**
     * @c 0x00 @n
     * When no bits are set, @b no information about the events is included. Use this if you want to learn only about
     * count of events that match #APP_MSG_RESPONSE_GETEVENTS_T.eventMask.
     */
    EVENT_INFO_NONE = 0,

    /**
     * @c 0x0F @n
     * Convenience value, enabling all the available different types of information.
     */
    EVENT_INFO_ALL = EVENT_INFO_INDEX | EVENT_INFO_TIMESTAMP | EVENT_INFO_ENUM | EVENT_INFO_DATA,

    /**
     * @c 0x80 @n
     * - May @b not be set when issuing the command in #APP_MSG_CMD_GETEVENTS_T.info
     * - May be set in the response #APP_MSG_RESPONSE_GETEVENTS_T.info.
     * .
     * When set, indicates more events matching #APP_MSG_RESPONSE_GETEVENTS_T.eventMask are available, that did not fit
     * in the response.
     */
    EVENT_INFO_MORE = 1 << 7
} EVENT_INFO_T;

/**
 * The different types of data that is sampled periodically.
 * @see APP_MSG_ID_GETPERIODICDATA
 */
typedef enum APP_MSG_PERIODICDATA_TYPE_S {
    APP_MSG_PERIODICDATA_TYPE_TEMPERATURE = 0x01, /** @c 0x01 @n Temperature */
    APP_MSG_PERIODICDATA_TYPE_HUMIDITY = 0x02, /** @c 0x02 @n Humidity */

    /** @c 2 @n Number of possible types of periodic data. Not to be used as a possible event. Use this in for loops or to define array sizes. */
    APP_MSG_PERIODICDATA_TYPE_COUNT = 2,

    /**
     * @c 0x03 @n
     * Convenience value, selecting all available types.
     */
    APP_MSG_PERIODICDATA_TYPE_ALL = APP_MSG_PERIODICDATA_TYPE_TEMPERATURE | APP_MSG_PERIODICDATA_TYPE_HUMIDITY
} APP_MSG_PERIODICDATA_TYPE_T;

/**
 * The different formats periodic data is represented in.
 * @see APP_MSG_ID_GETPERIODICDATA
 */
typedef enum APP_MSG_PERIODICDATA_FORMAT_S {
    /**
     * Each data sample takes up 1 or more full bytes:
     * - For temperature, each sample takes up two bytes, LSByte first. The value is expressed in deci-Celsius.
     *  For example, a temperature of 34.5C is encoded as 345 decimal, giving two bytes 59h 01h.
     * - For humidity, each sample takes up 1 byte. The value is rounded to the nearest halve percentage, then
     *  expressed in halve-percentages. For example, a humidity of 45.6% is encoded as 91, giving 1 byte 5Bh.
     * @note This type can be chosen with any chosen combination of #APP_MSG_PERIODICDATA_TYPE_T values.
     */
    APP_MSG_PERIODICDATA_FORMAT_FULL = 0,

    /**
     * The data is returned as it is stored in memory: an exact copy of the raw bytes from the non-volatile memories
     * EEPROM and FLASH used for data storage are returned. The data may be converted, mapped, lossless or lossy
     * compressed. It is up to the tag reader to know and understand the storage format and to be able to convert it
     * back to the sampled data.
     * @warning While this allows for much faster transmission of all the data, it also ties the implementation of the
     *  firmware application much tighter to the implementation of the tag reader host application.
     * @note This type can only be chosen in combination with #APP_MSG_PERIODICDATA_TYPE_ALL
     */
    APP_MSG_PERIODICDATA_FORMAT_RAW = 1,
} APP_MSG_PERIODICDATA_FORMAT_T;

/**
 * Possible resolutions for the Temperature Sensor, used in #APP_MSG_CMD_MEASURETEMPERATURE_T.resolution
 * @note Regardless of the resolution, the accuracy of the sensor is always +-0.3°C between 0°C and 40°C,
 *  and +-0.5°C between -40°C to +85°C.
 * @see APP_MSG_ID_MEASURETEMPERATURE
 */
typedef enum APP_MSG_TSEN_RESOLUTION {
    APP_MSG_TSEN_RESOLUTION_7BITS = 2, /*!< @c 0x02 @n 7 bits resolution: +-0.8°C; requires a conversion time of 4 ms. */
    APP_MSG_TSEN_RESOLUTION_8BITS = 3, /*!< @c 0x03 @n 8 bits resolution: +-0.4°C; requires a conversion time of 7 ms. */
    APP_MSG_TSEN_RESOLUTION_9BITS = 4, /*!< @c 0x04 @n 9 bits resolution: +-0.2°C; requires a conversion time of 14 ms. */
    APP_MSG_TSEN_RESOLUTION_10BITS = 5, /*!< @c 0x05 @n 10 bits resolution: +-0.1°C; requires a conversion time of 26 ms. */
    APP_MSG_TSEN_RESOLUTION_11BITS = 6, /*!< @c 0x06 @n 11 bits resolution: +-0.05°C; requires a conversion time of 50 ms. */
    APP_MSG_TSEN_RESOLUTION_12BITS = 7 /*!< @c 0x07 @n 12 bits resolution: +-0.025°C; requires a conversion time of 100 ms. */
} APP_MSG_TSEN_RESOLUTION_T;

/* -------------------------------------------------------------------------------- */

#pragma pack(push, 1)

/** @see APP_MSG_ID_GETMEASUREMENTS */
typedef struct APP_MSG_CMD_GETMEASUREMENTS_S {
    /**
     * Unit: number of samples.
     * - A value of 0 returns the oldest samples.
     * - Any other value denotes the number of old samples to skip.
     * .
     * @note This command has to be issued multiple times using different offsets, each time retrieving part of the
     *   stored samples.
     */
    uint16_t offset;
} APP_MSG_CMD_GETMEASUREMENTS_T;

/** @see APP_MSG_ID_SETCONFIG */
typedef struct APP_MSG_CMD_SETCONFIG_S {
    /**
     * The absolute current time in epoch seconds.
     */
    uint32_t currentTime;

    /**
     * The time between two measurements, set in seconds.
     * @note: A value of 0 disables taking measurements: after entering a power save mode,
     *   the chip can then only wake up when an NFC field is present.
     */
    uint16_t interval;

    /**
     * Only looked at when @c interval is strict positive.
     * Time in seconds to wait before the first measurement is made.
     * - If equal to @c 0, a first measurement will be made immediately (at @c currentTime).
     * - The special value #APP_MSG_DELAY_START_INDEFINITELY is used to defer the start indefinitely: the configuration
     *  will be fully stored, but no measurements will be made. To save battery, the IC will shut down when no NFC
     *  field is present; this means the IC cannot keep track of time during that period. Use the command with message
     *  id #APP_MSG_ID_START to make a first measurement.
     * - Any other value will cause the IC to minimize power while still keeping track of time.
     * .
     * Regardless of when the first measurement is taken, subsequent measurements will be spaced @c interval seconds
     * apart.
     */
    uint32_t startDelay;

    /**
     * Only looked at when @c interval is strict positive.
     * Time in seconds to run after the first measurement.
     * - A value of @c 0 indicates to run indefinitely, i.e. until storage is full, until the battery dies, or until a
     *  new configuration is given.
     * - Any other value gives a hard stop after that many seconds.
     * .
     * For example, with @c interval equal to @c 10 and @c runningTime equal to @c 42, a total of @c 5 measurements will
     * be made.
     * When the time expires, the IC will shut down and will no longer keep track of time. All measurements are retained
     * and can be retrieved.
     */
    uint32_t runningTime;

    /**
     * A validity constraint. The minimum value in deci-Celsius degrees for each measured temperature value.
     * - The entire batch of stored samples is considered valid if all samples comply with this constraint.
     * - The entire batch of stored samples is considered invalid if one or more samples do not comply with this
     *  constraint.
     * .
     * @note If @c validMinimum is not less than @c validMaximum, all samples are considered valid.
     */
    int16_t validMinimum;

    /**
     * A validity constraint. The minimum value in deci-Celsius degrees for each measured temperature value.
     * - The entire batch of stored samples is considered valid if all samples comply with this constraint.
     * - The entire batch of stored samples is considered invalid if one or more samples do not comply with this
     *  constraint.
     * .
     * @note If @c validMinimum is not less than @c validMaximum, all samples are considered valid.
     */
    int16_t validMaximum;
} APP_MSG_CMD_SETCONFIG_T;

/** @see APP_MSG_ID_MEASURETEMPERATURE */
typedef struct APP_MSG_CMD_MEASURETEMPERATURE_S {
    uint8_t resolution; /**< Type: #APP_MSG_TSEN_RESOLUTION_T */
} APP_MSG_CMD_MEASURETEMPERATURE_T;

/** @see APP_MSG_ID_GETEVENTS */
typedef struct APP_MSG_CMD_GETEVENTS_S {
    /**
     * Unit: number of events.
     * - A value of 0 returns the oldest events
     * - Any other value denotes the number of old samples to skip. Only events that are withheld using @c eventMask
     *  are counted.
     * .
     * @note This command may have to be issued multiple times using different offsets, each time retrieving part of the
     *   stored events.
     */
    uint16_t index;

    /**
     * A bitmask of OR'd events of type #APP_MSG_EVENT_T.
     * - Only events which have been set in the mask are retrieved.
     * - a value of @c 0 is treated as if @code (1 << APP_MSG_EVENT_COUNT) - 1 @endcode is given.
     * .
     */
    uint32_t eventMask;

    /**
     * A bitmask of OR'd event information types of type #EVENT_INFO_T.
     * - Only data from information types which have been set in the mask are returned.
     * - Use the value #EVENT_INFO_NONE to retrieve just the count of events matching @c eventMask.
     * .
     */
    uint8_t info;
} APP_MSG_CMD_GETEVENTS_T;

/** @see APP_MSG_ID_GETPERIODICDATA */
typedef struct APP_MSG_CMD_GETPERIODICDATA_S {
    /**
     * A bitmask of OR'd values of type #APP_MSG_PERIODICDATA_TYPE_T.
     * - Only data from the types which have been set in the mask are returned.
     * - When multiple data types have been set - i.e. more than one bit - the data is interleaved: first a data sample
     *  of the type indicated by the lowest set bit, second a data sample of the type indicated by the second set bit,
     *  and so on.
     * .
     */
    uint8_t which;

    /**
     * The enumeration value of type #APP_MSG_PERIODICDATA_FORMAT_T, indicating the format of the retrieved data.
     * When this format is not compatible with the value @c which, only an error response is generated:
     * #MSG_RESPONSE_RESULTONLY_T
     */
    uint8_t format;

    /**
     * - If @c format equals @c APP_MSG_PERIODICDATA_FORMAT_FULL:
     *  Unit: number of samples.
     *  - A value of 0 returns the oldest samples.
     *  - Any other value denotes the number of old samples to skip.
     *  .
     * - If @c format equals @c APP_MSG_PERIODICDATA_FORMAT_RAW:
     *  Unit: number of bytes.
     *  - A value of 0 returns the oldest bytes.
     *  - Any other value denotes the number of bytes to skip.
     *  .
     * .
     * @note This command has to be issued multiple times using different offsets, each time retrieving part of the
     *   stored samples.
     */
    uint16_t offset;
} APP_MSG_CMD_GETPERIODICDATA_T;

/* ------------------------------------------------------------------------- */

/**
 * @see APP_MSG_ID_GETMEASUREMENTS
 *
 * -# If a valid command was received, and temperature values are available, the structure is
 *  appended with extra data for each reported temperature:
 *  @dot
 *      digraph "GetMeasurements response A" {
 *          graph [rankdir = "LR"];
 *          node [fontsize = "14" shape = "ellipse"];
 *          edge [];
 *
 *          "node_response" [label = "<f1> \n result = MSG_OK \n\n | <f2> \n offset \n\n | <f3> \n count \n\n| <f4> \n 00h, 00h, 00h \n\n | <f5> \n Extra data \n appended to structure \n\n" shape = "record"];
 *          "node_command2" [label = "<f0> Copied from GetMeasurements command - possibly modified" shape = "record", style = dashed];
 *          "node_count" [label = "<f0> # of measurements reported \n in this response" shape = "record", style = dashed];
 *          "node_blocks" [label = "<f1> \n temperature value \n of #offset \n in deci-Celsius, 2 bytes \n\n | <f2> \n temperature value \n of #offset + 1 \n in deci-Celsius, 2 bytes \n\n | <f3> ... | <f4> \n temperature value \n of #offset + #count - 1\n in deci-Celsius, 2 bytes \n\n" shape = "record"];
 *
 *          "node_response":f2 -> "node_command2":f0 [id = 1, style = dashed, dir = back];
 *          "node_response":f3 -> "node_count":f0 [id = 2, style = dashed, dir = back];
 *          "node_response":f5 -> "node_blocks":f1 [id = 3];
 *          "node_response":f5 -> "node_blocks":f2 [id = 4];
 *          "node_response":f5 -> "node_blocks":f3 [id = 5];
 *          "node_response":f5 -> "node_blocks":f4 [id = 6];
 *      }
 *  @enddot
 *  @n@n
 *
 * -# If the command was rejected, or if no temperature values are available, the response consists of just the structure
 *  without trailing extra bytes:
 *  @dot
 *      digraph "GetMeasurements response B" {
 *          graph [rankdir = "LR"];
 *          node [fontsize = "14" shape = "ellipse"];
 *          edge [];
 *
 *          "node_response" [label = "<f1> \n result = MSG_OK \n\n | <f2> \n offset \n\n | <f3> \n count = 0 \n\n| <f4> \n zero \n\n" shape = "record"];
 *          "node_command1" [label = "<f0> Copied from GetMeasurements command - unmodified " shape = "record", style = dashed];
 *
 *          "node_response":f2 -> "node_command1":f0 [id = 1, style = dashed, dir = back];
 *      }
 *  @enddot
 *  @n@n
 *
 * -# Or if the command was rejected, only a #MSG_RESPONSE_RESULTONLY_T structure is returned:
 *  @dot
 *      digraph "GetMeasurements response C" {
 *          graph [rankdir = "LR"];
 *          node [fontsize = "14" shape = "ellipse"];
 *          edge [];
 *
 *          "node_response" [label = "<f1> \n result = MSG_ERR_INVALID_PARAMETER \n\n" shape = "record"];
 *      }
 *  @enddot
 */
typedef struct APP_MSG_RESPONSE_GETMEASUREMENTS_S {
    /**
     * The command result.
     * Only when @c result equals #MSG_OK, the other fields in this response are valid.
     */
    uint32_t result;

    /**
     * Unit: number of samples.
     * Defines the sequence number of the first data value that follows.
     * @note This may differ from the offset value given in the corresponding command: #APP_MSG_CMD_GETMEASUREMENTS_T.offset
     */
    uint16_t offset;

    /**
     * The number of values that follow @b after the padding bytes. This number can be @c 0.
     * Immediately following this structure is an array of @c count elements, each element 16 bits wide. Each element
     * holds one measurement value in deci-Celsius degrees. The total size of the response is thus variable and equals
     * @code sizeof(APP_MSG_RESPONSE_GETMEASUREMENTS_T) + sizeof(int16_t) * count @endcode
     * @note There are no implicit padding bytes before the values, only the explicit bytes below.
     * @note The timestamp for each measurement can be reconstructed using #APP_MSG_RESPONSE_GETCONFIG_T.startTime and
     *  #APP_MSG_RESPONSE_GETCONFIG_T.interval, the value of @c offset and the position in the array of values that
     *  follow after this structure.
     */
    uint8_t count;

    /**
     * Padding bytes. Must be @c 0.
     * @note Added to ensure size is a multiple of 2. Added solely to ease ARM SW development, and left at 3 for
     *  backwards compatibility.
     * @note Directly after these padding bytes, the measurements are listed, to be interpreted as
     *  @code int16_t data[count] @endcode
     */
    uint8_t zero[3];

    //int16_t data[count];
} APP_MSG_RESPONSE_GETMEASUREMENTS_T;

/** @see APP_MSG_ID_GETCONFIG */
typedef struct APP_MSG_RESPONSE_GETCONFIG_S {
    /**
     * The command result.
     * Only when @c result equals #MSG_OK, the contents below this field are valid.
     */
    uint32_t result;

    /* -------------------------------------------------------------------------------- */

    /** The value as given by #APP_MSG_CMD_SETCONFIG_T.currentTime, with @c 0 as default value. */
    uint32_t configTime;

    /** The value as given by #APP_MSG_CMD_SETCONFIG_T.interval, with @c 0 as default value. */
    uint16_t interval;

    /** The value as given by #APP_MSG_CMD_SETCONFIG_T.startDelay, with @c 0 as default value. */
    uint32_t startDelay;

    /** The value as given by #APP_MSG_CMD_SETCONFIG_T.runningTime, with @c 0 as default value. */
    uint32_t runningTime;

    /** The value as given by #APP_MSG_CMD_SETCONFIG_T.validMinimum, with @c 32767 as default value. */
    int16_t validMinimum;

    /** The value as given by #APP_MSG_CMD_SETCONFIG_T.validMaximum, with @c -32768 as default value. */
    int16_t validMaximum;

    /* -------------------------------------------------------------------------------- */

    /** The absolute minimum value of all temperature measurements. */
    int16_t attainedMinimum;

    /** The absolute maximum value of all temperature measurements. */
    int16_t attainedMaximum;

    /** The number of measurements available. */
    uint16_t count;

    /**
     * A bitmask of OR'd events of type #APP_MSG_EVENT_T.
     * - Multiple bits indicating a state change can be set simultaneously: in that case, the highest value bit
     *  indicates the current state.
     * - Multiple bits indicating a failure or achievement can be set simultaneously.
     * .
     */
    uint32_t status;

    /** The time in epoch seconds when the first measurement was taken. @c 0 if no measurement has yet been taken. */
    uint32_t startTime;

    /**
     * The current time as known by the IC. This may or may not reflect the current absolute time in epoch seconds:
     * The IC may have stopped keeping track of time:
     * - because the battery died,
     * - because an #APP_MSG_CMD_SETCONFIG_T.interval was set to @c 0,
     * - because the allowed time as set with #APP_MSG_CMD_SETCONFIG_T.runningTime has elapsed.
     * - because the #APP_MSG_CMD_SETCONFIG_T.startDelay was given the special value #APP_MSG_DELAY_START_INDEFINITELY
     *  and no command with message id #APP_MSG_ID_START has been given yet.
     * - because a reset pulse was given to the IC.
     * .
     * In these cases, @c currentTime reflects the time since last waking up again after being shut down.
     * In all other cases the epoch time in seconds was given to the IC using a command with message ID
     * #APP_MSG_ID_SETCONFIG or #APP_MSG_ID_START and the IC kept up to and including now track of time. The value
     * returned here then reflects the current absolute epoch time in seconds.
     * @note Using the other fields in this structure the correct type of the value can be deduced. Typical epoch
     *  values will also be a lot larger than any running time since a last reset pulse: any value less than
     *  @c configTime indicates logging has stopped and the device went to power-off mode or has experienced a power
     *  loss.
     */
    uint32_t currentTime;
} APP_MSG_RESPONSE_GETCONFIG_T;

/** @see APP_MSG_ID_MEASURETEMPERATURE */
typedef struct APP_MSG_RESPONSE_MEASURETEMPERATURE_S {
    /**
     * The command result.
     * Only when @c result equals #MSG_OK, the contents of @c data is valid.
     */
    uint32_t result;

    int16_t temperature; /**< The measured temperature in deci-Celsius degrees. */
} APP_MSG_RESPONSE_MEASURETEMPERATURE_T;

/**
 * @see APP_MSG_ID_GETEVENTS
 *
 * Depending on the value given in #APP_MSG_CMD_GETEVENTS_T.info, the response will have a different stream of bytes
 * 'data' added right after the last field of this structure. The diagrams below give some examples what to expect:
 *
 * -# If only the count is requested, the response is most simple:
 *  @dot
 *      digraph "GetEvents response A" {
 *          graph [rankdir = "LR"];
 *          node [fontsize = "14" shape = "ellipse"];
 *          edge [];
 *
 *          "node_response" [label = "<f1> \n index \n\n | <f2> \n eventMask \n\n | <f3> \n info = NONE \n\n| <f4> \n count \n\n" shape = "record"];
 *          "node_command1" [label = "<f0> Copied from GetEvents command - unmodified " shape = "record", style = dashed];
 *          "node_count" [label = "<f0> Total # of events stored" shape = "record", style = dashed];
 *
 *          "node_response":f1 -> "node_command1":f0 [id = 0, style = dashed, dir = back];
 *          "node_response":f2 -> "node_command1":f0 [id = 1, style = dashed, dir = back];
 *          "node_response":f3 -> "node_command1":f0 [id = 2, style = dashed, dir = back];
 *          "node_response":f4 -> "node_count":f0 [id = 3, style = dashed, dir = back];
 *  }
 *  @enddot
 *  @n@n@n
 *
 * -# If for each event both the event value and the timestamp on which it occurred is requested, the structure is
 *  appended with extra data for each reported event:
 *  @dot
 *      digraph "GetEvents response B" {
 *          graph [rankdir = "LR"];
 *          node [fontsize = "14" shape = "ellipse"];
 *          edge [];
 *
 *          "node_response" [label = "<f1> \n index \n\n | <f2> \n eventMask \n\n | <f3> \n info = TIMESTAMP \n + ENUM \n\n| <f4> \n count \n\n | <f5> \n Extra data \n appended to structure \n\n" shape = "record"];
 *          "node_command1" [label = "<f0> Copied from GetEvents command - unmodified " shape = "record", style = dashed];
 *          "node_command2" [label = "<f0> Copied from GetEvents command - possibly modified" shape = "record", style = dashed];
 *          "node_count" [label = "<f0> # of events reported \n in this response" shape = "record", style = dashed];
 *          "node_blocks" [label = "<f1> first reported event | <f2> second reported event | <f3> ... | <f4> last reported event" shape = "record"];
 *          "node_block" [label = "<f1> \n timestamp \n 4 bytes \n\n | <f2> \n log_2 of enum value \n 1 byte \n\n" shape = "record"];
 *
 *          "node_response":f1 -> "node_command1":f0 [id = 0, style = dashed, dir = back];
 *          "node_response":f2 -> "node_command1":f0 [id = 1, style = dashed, dir = back];
 *          "node_response":f3 -> "node_command2":f0 [id = 2, style = dashed, dir = back];
 *          "node_response":f4 -> "node_count":f0 [id = 3, style = dashed, dir = back];
 *          "node_response":f5 -> "node_blocks":f1 [id = 4];
 *          "node_response":f5 -> "node_blocks":f2 [id = 5];
 *          "node_response":f5 -> "node_blocks":f3 [id = 6];
 *          "node_response":f5 -> "node_blocks":f4 [id = 7];
 *          "node_blocks":f1 -> "node_block":f1 [id = 8, taillabel = "\n for each \n reported event"];
 *          "node_blocks":f1 -> "node_block":f2 [id = 9];
 *  }
 *  @enddot
 *  @n@n@n
 *
 * -# In case all event information is requested, a GetEvents response is assembled as:
 *  @dot
 *      digraph "GetEvents response C" {
 *          graph [rankdir = "LR"];
 *          node [fontsize = "14" shape = "ellipse"];
 *          edge [];
 *
 *          "node_response" [label = "<f1> \n index \n\n | <f2> \n eventMask \n\n | <f3> \n info = INDEX \n + TIMESTAMP \n + ENUM \n + DATA \n\n| <f4> \n count \n\n | <f5> \n Extra data \n appended to structure \n\n" shape = "record"];
 *          "node_command1" [label = "<f0> Copied from GetEvents command - unmodified " shape = "record", style = dashed];
 *          "node_command2" [label = "<f0> Copied from GetEvents command - possibly modified" shape = "record", style = dashed];
 *          "node_count" [label = "<f0> # of events reported \n in this response" shape = "record", style = dashed];
 *          "node_blocks" [label = "<f1> first reported event | <f2> second reported event | <f3> ... | <f4> last reported event" shape = "record"];
 *          "node_block" [label = "<f1> \n index \n 2 bytes \n\n | <f2> \n timestamp \n 4 bytes \n\n | <f3> \n log_2 of enum value \n 1 byte \n\n | <f4> \n extra data depending on specific event: \n variable # of bytes, from 0 up to 127 \n\n" shape = "record"];
 *
 *          "node_response":f1 -> "node_command1":f0 [id = 0, style = dashed, dir = back];
 *          "node_response":f2 -> "node_command1":f0 [id = 1, style = dashed, dir = back];
 *          "node_response":f3 -> "node_command2":f0 [id = 2, style = dashed, dir = back];
 *          "node_response":f4 -> "node_count":f0 [id = 3, style = dashed, dir = back];
 *          "node_response":f5 -> "node_blocks":f1 [id = 3];
 *          "node_response":f5 -> "node_blocks":f2 [id = 4];
 *          "node_response":f5 -> "node_blocks":f3 [id = 5];
 *          "node_response":f5 -> "node_blocks":f4 [id = 6];
 *          "node_blocks":f1 -> "node_block":f1 [id = 7, taillabel = "\n for each \n reported event"];
 *          "node_blocks":f1 -> "node_block":f2 [id = 8];
 *          "node_blocks":f1 -> "node_block":f3 [id = 9];
 *          "node_blocks":f1 -> "node_block":f4 [id = 10];
 *  }
 *  @enddot
 */
typedef struct APP_MSG_RESPONSE_GETEVENTS_S {
    /**
     * Unit: number of events.
     * The value as given by #APP_MSG_CMD_GETEVENTS_T.index.
     */
    uint16_t index;

    /**
     * A bitmask of OR'd events of type #APP_MSG_EVENT_T.
     * The value as given by #APP_MSG_CMD_GETEVENTS_T.eventMask, with @c 0 replaced by
     * @code (1 << APP_MSG_EVENT_COUNT) - 1 @endcode.
     */
    uint32_t eventMask;

    /**
     * A bitmask of OR'd event information types of type #EVENT_INFO_T.
     * The value as given by #APP_MSG_CMD_GETEVENTS_T.info, possibly extended by setting
     * #EVENT_INFO_MORE.
     * - If equal to #EVENT_INFO_NONE @b no data is following this structure.
     * - If #EVENT_INFO_MORE is set, issue a new command using @c index @c + @c count as the new value
     *  for @c index.
     * .
     */
    uint8_t info;

    /**
     * The number of events that match @c eventMask that are reported by this response. This number can be @c 0.
     * - This number may be less than the total number of events matching @c eventMask - as indicated by
     *  #EVENT_INFO_MORE being set or not in @c info.
     * .
     * Immediately following this structure is an array of bytes, with extra information per event, as indicated by
     * @c info.
     * - Depending on the bits set in @c info, the number of bytes added will vary;
     *  if #EVENT_INFO_DATA is set, the number of added bytes can even differ @b per @b event.
     * - The order in which the enumeration values are listed - and their enumeration value - in #EVENT_INFO_T
     *  determines the order in which the information is given.
     * .
     * The total size of the response is thus variable and must be deduced from this value @c count together with
     * @c info.
     */
    uint16_t count;

    //uint8_t data[...]; - extra information per event, as indicated in the info struct member
} APP_MSG_RESPONSE_GETEVENTS_T;


/** @see APP_MSG_ID_GETPERIODICDATA */
typedef struct APP_MSG_RESPONSE_GETPERIODICDATA_S {
    /**
     * The command result.
     * Only when @c result equals #MSG_OK, the other fields in this response are valid.
     */
    uint32_t result;

    /**
     * A bitmask of OR'd values of type #APP_MSG_PERIODICDATA_TYPE_T
     * The value as given by #APP_MSG_CMD_GETPERIODICDATA_T.which, possibly reduced to only list the available types.
     */
    uint8_t which;

    /** The value as given by #APP_MSG_CMD_GETPERIODICDATA_T.format */
    uint8_t format;

    /**
     * A value smaller than or equal to #APP_MSG_CMD_GETPERIODICDATA_T.offset, with the unit defined by @c format.
     * @note Directly hereafter, the data is given, to be interpreted per the value set in @c which and @c format.
     *  It is possible no bytes follow, indicating no data matching the filters set in #APP_MSG_CMD_GETPERIODICDATA_T
     *  are available.
     *  There are no padding bytes before the values.
     * @warning The amount of bytes or the number of samples given is not provided and must be derived from the
     *  interpretation of the field values in this structure and the extra bytes appended hereafter.
     * @note The timestamp for each measurement can be reconstructed using #APP_MSG_RESPONSE_GETCONFIG_T.startTime and
     *  #APP_MSG_RESPONSE_GETCONFIG_T.interval, and the sequence value of each data sample.
     *
     * @par Example
     *  when:
     *  - @code which == APP_MSG_PERIODICDATA_TYPE_TEMPERATURE | APP_MSG_PERIODICDATA_TYPE_HUMIDITY @endcode
     *  - @code format == APP_MSG_PERIODICDATA_FORMAT_FULL @endcode
     *  - just two measurements have been made: first a temperature of 34.5C and a humidity of 45.6%, then a
     *      temperature of 56.7 and a humidity of 67.8%
     *  .
     *  A total of 6 bytes will be added:
     *  @code T1 H1 T2 H2 -> 345 91 567 136 -> (59h 01h) 5Bh (37h 02h) 88h @endcode
     */
    uint16_t offset;

    //byte data[...];
} APP_MSG_RESPONSE_GETPERIODICDATA_T;

#pragma pack(pop)

#endif /** @} */
