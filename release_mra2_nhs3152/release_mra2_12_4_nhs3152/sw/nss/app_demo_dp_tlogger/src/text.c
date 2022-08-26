/*
 * Copyright 2015-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include <string.h>
#include "storage/storage.h"
#include "event/event.h"
#include "event_tag.h"
#include "temperature.h"
#include "memory.h"
#include "msghandler_protocol.h"
#include "text.h"

/* ------------------------------------------------------------------------- */

/** String template for each possible event. */
static const char * sEventStrings[EVENT_TAG_COUNT] = {
    "Empty. Not yet configured.", /* EVENT_TAG_PRISTINE */
    "Empty. Configured but not yet started.", /* EVENT_TAG_CONFIGURED */
/*   0         1         2         3         4         5         6         7         8         9          */
/*   0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789 */
    "Running. No sample taken yet.", /* EVENT_TAG_STARTING */
    "Running for ___ _______. _____ samples logged.", /* EVENT_TAG_LOGGING */
    "Stopped. _____ samples logged.", /* EVENT_TAG_STOPPED */
    "ALERT: first high excursion after ___ ______.", /* EVENT_TAG_TEMPERATURE_TOO_HIGH */
    "ALERT: first low excursion after ___ ______.", /* EVENT_TAG_TEMPERATURE_TOO_LOW */
/*   0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789 */
/*   0         1         2         3         4         5         6         7         8         9          */
    "ALERT: battery is empty.", /* EVENT_TAG_BOD */
    "ALERT: no space left for storing samples.", /* EVENT_TAG_FULL */
    "ALERT: stopped after the configured time.", /* EVENT_TAG_EXPIRED */
    "ALERT: last I2C access failed.", /* EVENT_TAG_I2C_ERROR */
    "ALERT: last SPI access failed." /* EVENT_TAG_SPI_ERROR */
};

/* EVENT_TAG_LOGGING */
#define LOGGING_DURATION_VALUE_POS 14 /**< Last position of field */
#define LOGGING_DURATION_VALUE_LENGTH 3
#define LOGGING_DURATION_UNIT_POS 16 /**< Start position of field */
#define LOGGING_DURATION_UNIT_LENGTH 7

#define LOGGING_COUNT_VALUE_POS 29 /**< Last position of field */
#define LOGGING_COUNT_VALUE_LENGTH 5

/* EVENT_TAG_STOPPED */
#define STOPPED_COUNT_VALUE_POS 13 /**< Last position of field */
#define STOPPED_COUNT_VALUE_LENGTH 5

/* EVENT_TAG_TEMPERATURE_TOO_HIGH */
#define HIGH_DURATION_VALUE_POS 36 /**< Last position of field */
#define HIGH_DURATION_VALUE_LENGTH 3
#define HIGH_DURATION_UNIT_POS 38 /**< Start position of field */
#define HIGH_DURATION_UNIT_LENGTH 7

/* EVENT_TAG_TEMPERATURE_TOO_LOW */
#define LOW_DURATION_VALUE_POS 35 /**< Last position of field */
#define LOW_DURATION_VALUE_LENGTH 3
#define LOW_DURATION_UNIT_POS 37 /**< Start position of field */
#define LOW_DURATION_UNIT_LENGTH 7

/* ------------------------------------------------------------------------- */

/*                                         0         1         2          3         4          5         6 */
/*                                         012345678901234567890123456 7890123456789012 345678901234567890 */
static const char * s1TemperatureString = "Current temperature: ___._C";
static const char * s3TemperatureString = "Current temperature: ___._C\nMinimum: ___._C\nMaximum: ___._C";

#define CURRENT_TEMPERATURE_POS 25 /**< Last position of field */
#define MINIMUM_TEMPERATURE_POS 41 /**< Last position of field */
#define MAXIMUM_TEMPERATURE_POS 57 /**< Last position of field */

/* ------------------------------------------------------------------------- */

static void DeciValue2String(int deciValue, char * string);
static void Number2String(int remaining, char * end, int size);
static void Duration2String(int duration, char * countEnd, int maxCountLength, char * unitBegin, int maxUnitLength);

/* ------------------------------------------------------------------------- */

/** Helper function to print a number (decimal). */
static void Number2String(int remaining, char * end, int size)
{
    memset(end - size + 1, ' ', (size_t)size);
    *end = '0';
    while ((size > 0) && (remaining > 0)) {
        *end = (char)('0' + (remaining % 10));
        end--;
        remaining /= 10;
        size--;
    }
}

/**
 * Writes a human-readable version of a deci-value to the NFC buffer
 * @param value deci-value. For example, a value of 123 will result in "12.3"
 */
static void DeciValue2String(int deciValue, char * end)
{
    bool positive = true;

    if (deciValue < 0) {
        positive = false;
        deciValue *= -1;
    }

    Number2String(deciValue % 10, end, 1);

    end--;
    *end = '.';

    end--;
    Number2String(deciValue / 10, end, 3);

    if (!positive) {
        end -= 2; /* If negative, the absolute number can have at most two digits. */
        *end = '-';
    }
}

/** Helper function to print a duration based on seconds, minutes, hours or days, whichever makes most sense for humans. */
static void Duration2String(int duration, char * countEnd, int maxCountLength, char * unitBegin, int maxUnitLength)
{
    static const char sSeconds[] = "seconds";
    static const char sMinutes[] = "minutes";
    static const char sHours[] = "hours";
    static const char sDays[] = "days";
    const char * unit;
    int size;

    if (duration < 3 * 60) {
        unit = sSeconds;
        size = sizeof(sSeconds) - 1; /* exclude NUL byte */
    }
    else if (duration < 3 * 60 * 60) {
        duration /= 60;
        unit = sMinutes;
        size = sizeof(sMinutes) - 1; /* exclude NUL byte */
    }
    else if (duration < 3 * 60 * 60 * 24) {
        duration /= (60 * 60);
        unit = sHours;
        size = sizeof(sHours) - 1; /* exclude NUL byte */
    }
    else {
        duration /= (60 * 60 * 24);
        unit = sDays;
        size = sizeof(sDays) - 1; /* exclude NUL byte */
    }
    if (maxUnitLength < size) {
        size = maxUnitLength;
    }
    memset(unitBegin, ' ', (size_t)maxUnitLength);
    memcpy(unitBegin, unit, (size_t)size);

    Number2String(duration, countEnd, maxCountLength);
}

/* ------------------------------------------------------------------------- */

const char * Text_GetStatus(int * pLen)
{
    __attribute__ ((section(".noinit")))
    static char sString[TEXT_MAX_STATUS_LENGTH];
    const MEMORY_CONFIG_T * config = Memory_GetConfig();

    uint32_t index = EVENT_TAG_LAST_STATE;
    while ((index != EVENT_TAG_FIRST_STATE) && (!(config->status & (1U << index)))) {
        index--;
    }

    int count = Storage_GetCount();
    memset(sString, 0, TEXT_MAX_STATUS_LENGTH);
    strcpy(sString, sEventStrings[index]);
    switch (index) {
        case EVENT_TAG_LOGGING: {
            uint32_t startTime = 0; /* Default value if no event found. */
            (void)Event_GetFirstByTag(EVENT_TAG_LOGGING, NULL, NULL, NULL, &startTime);
            int duration = Chip_RTC_Time_GetValue(NSS_RTC) - (int)startTime;
            Duration2String(duration, sString + LOGGING_DURATION_VALUE_POS, LOGGING_DURATION_VALUE_LENGTH,
                            sString + LOGGING_DURATION_UNIT_POS, LOGGING_DURATION_UNIT_LENGTH);
            Number2String(count, sString + LOGGING_COUNT_VALUE_POS, LOGGING_COUNT_VALUE_LENGTH);
        } break;

        case EVENT_TAG_STOPPED:
            Number2String(count, sString + STOPPED_COUNT_VALUE_POS, STOPPED_COUNT_VALUE_LENGTH);
            break;

        default:
            /* There are no fields to populate. */
            break;
    }

    *pLen = TEXT_MAX_STATUS_LENGTH;
    return sString;
}

const char * Text_GetFailures(int * pLen)
{
    __attribute__ ((section(".noinit")))
    static char sString[(EVENT_TAG_LAST_FAILURE - EVENT_TAG_FIRST_FAILURE + 1) * TEXT_MAX_FAILURE_LENGTH];
    const MEMORY_CONFIG_T * config = Memory_GetConfig();

    uint32_t startTime = 0; /* Default value if no event found. */
    (void)Event_GetFirstByTag(EVENT_TAG_LOGGING, NULL, NULL, NULL, &startTime);

    memset(sString, 0, (EVENT_TAG_LAST_FAILURE - EVENT_TAG_FIRST_FAILURE + 1) * TEXT_MAX_FAILURE_LENGTH);
    char * p = sString;
    *pLen = 0;
    EVENT_TAG_T e = EVENT_TAG_FIRST_FAILURE;
    while (e <= EVENT_TAG_LAST_FAILURE) {
        if (config->status & (uint32_t)(1 << e)) {
            if (*pLen > 0) {
                *(p - 1) = '\n';
            }
            uint32_t eventTime = startTime; /* Default value if no event found. */
            Event_GetFirstByTag(e, NULL, NULL, NULL, &eventTime);
            strcpy(p, sEventStrings[e]);
            if (e == EVENT_TAG_TEMPERATURE_TOO_HIGH) {
                int duration = (int)eventTime - (int)startTime;
                Duration2String(duration, p + HIGH_DURATION_VALUE_POS, HIGH_DURATION_VALUE_LENGTH,
                                p + HIGH_DURATION_UNIT_POS, HIGH_DURATION_UNIT_LENGTH);
            }
            else if (e == EVENT_TAG_TEMPERATURE_TOO_LOW) {
                int duration = (int)eventTime - (int)startTime;
                Duration2String(duration, p + LOW_DURATION_VALUE_POS, LOW_DURATION_VALUE_LENGTH,
                                p + LOW_DURATION_UNIT_POS, LOW_DURATION_UNIT_LENGTH);
            }
            p += TEXT_MAX_FAILURE_LENGTH;
            *pLen += TEXT_MAX_FAILURE_LENGTH;
        }
        e++;
    }

    return sString;
}

const char * Text_GetTemperature(int * pLen)
{
    __attribute__ ((section(".noinit")))
    static char sString[TEXT_MAX_TEMPERATURE_LENGTH];
    const MEMORY_CONFIG_T * config = Memory_GetConfig();

    memset(sString, 0, TEXT_MAX_TEMPERATURE_LENGTH);
    if (config->attainedMinimum < config->attainedMaximum) {
        strcpy(sString, s3TemperatureString);
        DeciValue2String(config->attainedMinimum, &sString[MINIMUM_TEMPERATURE_POS]);
        DeciValue2String(config->attainedMaximum, &sString[MAXIMUM_TEMPERATURE_POS]);
    }
    else {
        strcpy(sString, s1TemperatureString);
    }
    DeciValue2String(Temperature_Get(), &sString[CURRENT_TEMPERATURE_POS]);

    *pLen = TEXT_MAX_TEMPERATURE_LENGTH;
    return sString;
}
