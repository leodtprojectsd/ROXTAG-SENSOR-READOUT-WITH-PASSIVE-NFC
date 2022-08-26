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

#include <stddef.h>
#include "board.h"
#include "storage/storage.h"
#include "event/event.h"
#include "event_tag.h"
#include "memory.h"
#include "msghandler_protocol.h"
#include "validate.h"

/* ------------------------------------------------------------------------- */

typedef struct VALIDATE_S {
    uint32_t previousTemperatureTooLow : 1;
    uint32_t previousTemperatureTooHigh : 1;
    uint32_t unused : 30;
} VALIDATE_T;

/**
 * Dummy variable to test whether the #VALIDATE_T size is exactly one 32-bit word.
 * If not equal, the dummy variable will have a negative array size and the compiler will raise an error
 * similar to:
 *   ../src/validate.c:24:29: error: size of array 'sTestStructSize.' is negative
 * @{
 */
static char sTestStructSize[(sizeof(VALIDATE_T) == sizeof(uint32_t)) ? 1 : -1] __attribute__((unused));

/**
 * Maintains whether an excursion is ongoing. When this ends, we can update the corresponding event with extra data - the end-timestamp.
 */
static VALIDATE_T * spValidate = NULL;

/* ------------------------------------------------------------------------- */

void Validate_Init(uint32_t * pWorkspace)
{
    spValidate = (VALIDATE_T *)pWorkspace;

    /* Nothing else to be done in this demo application.
     * A full blown application may want to also resurrect the previous state of its validation algorithm.
     */
}

void Validate_DeInit(void)
{
    /* Nothing to be done in this demo application. spValidate is already up to date.
     * A full blown application may need to take extra actions preserve the current state of its validation algorithm.
     */
}

void Validate_Reset(void)
{
    if (spValidate != NULL) {
        spValidate->previousTemperatureTooLow = false;
        spValidate->previousTemperatureTooHigh = false;
    }

    /* Nothing else to be done in this demo application.
     * A full blown application may want to re-initialize its validation algorithm.
     */
}

void Validate_Temperature(int16_t temperature)
{
    if (temperature == APP_MSG_TEMPERATURE_PLACEHOLDER_VALUE) {
        /* Nothing to update. */
    }
    else {
        Memory_SetAttainedValue(temperature);

        const MEMORY_CONFIG_T * config = Memory_GetConfig();
        if (config->cmd.validMinimum < config->cmd.validMaximum) { /* if we have a valid constraint set */
            if (temperature < config->cmd.validMinimum) {
                /* When the previous temperature was within bounds, but this one isn't, a new event must be created. */
                bool createNewEvent = !spValidate->previousTemperatureTooLow;
                Memory_AddToState(APP_MSG_EVENT_TEMPERATURE_TOO_LOW, !createNewEvent);
                spValidate->previousTemperatureTooLow = true;
            }
            else if (temperature > config->cmd.validMaximum) {
                /* When the previous temperature was within bounds, but this one isn't, a new event must be created. */
                bool createNewEvent = !spValidate->previousTemperatureTooHigh;
                Memory_AddToState(APP_MSG_EVENT_TEMPERATURE_TOO_HIGH, !createNewEvent);
                spValidate->previousTemperatureTooHigh = true;
            }
            else {
                uint8_t tag = EVENT_TAG_COUNT; /* An invalid value */
                if (spValidate->previousTemperatureTooLow) {
                    tag = EVENT_TAG_TEMPERATURE_TOO_LOW;
                } else if (spValidate->previousTemperatureTooHigh) {
                    tag = EVENT_TAG_TEMPERATURE_TOO_HIGH;
                }
                if (tag < EVENT_TAG_COUNT) {
                    /* The excursion ended. Fetch the previously created event, and update the extra data. */
                    int offset = -1;
                    uint8_t len = 0;
                    Event_GetLastByTag(tag, &offset, &len, NULL, NULL);
                    if ((offset >= 0) && (len > 0)) {
                        uint32_t now = (uint32_t)Chip_RTC_Time_GetValue(NSS_RTC);
                        Chip_EEPROM_Write(NSS_EEPROM, offset, &now, len);
                    }
                }
                spValidate->previousTemperatureTooLow = false;
                spValidate->previousTemperatureTooHigh = false;
            }
        }
    }

    /* Nothing else to be done in this demo application.
     * A full blown application may want to implement a more complicated algorithm tailored to the product it
     * is attached to, to decide when to mark the end result a monitoring session as invalid / bad / expired / ...
     */
}

void Validate_NewEvents(uint32_t newEvents)
{
    (void)newEvents; /* suppress [-Wunused-parameter] */

    /* Nothing to be done in this demo application.
     * A full blown application may want to adapt the validation algorithm based on the current state or the different
     * reported error conditions.
     */
}
