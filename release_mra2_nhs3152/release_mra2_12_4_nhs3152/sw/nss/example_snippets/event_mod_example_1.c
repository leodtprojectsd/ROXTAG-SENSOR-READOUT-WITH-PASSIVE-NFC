/*
 * Copyright 2017 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"
#include "event/event.h"

//! [event_mod_example_1_defines]
#define GOOD 0x12
#define BAD 0x34
//! [event_mod_example_1_defines]

//! [event_mod_example_1_variables]
static volatile unsigned int sTimestamp;
static volatile unsigned int sCallCount;
//! [event_mod_example_1_variables]

//! [event_mod_example_1_callback]
/**
 * @see EVENT_CB
 * @see pEvent_Cb_t
 */
void EventCb(uint8_t tag, int offset, uint8_t len, unsigned int index, uint32_t timestamp, uint32_t context)
{
    sCallCount++;
    switch (context) {
        case 0xAB: /* Retrieved by index. */
            sTimestamp = timestamp;
            break;

        case 0xCD: /* Retrieved by timestamp. */
            ASSERT((index == 2) || (index == 3) || (index == 4));
            /* Retrieving "gOoD", "bAd" and "BAD", in that order. */
            break;

        case 0xEF: /* Retrieved by tag. */
            ASSERT(tag == GOOD);
            ASSERT(len == 4);
            ASSERT(offset > 0);
            /* Retrieving "good", "gOoD", and "GOOD", in that order. */
            break;
    }
}
//! [event_mod_example_1_callback]

void event_mod_example_1(void)
{
//! [event_mod_example_1]
    Event_Init(true);

    Chip_Clock_System_BusyWait_ms(4000);
    Event_Set(GOOD, "good", 4);
    Event_Set(BAD, "bad", 3);

    Chip_Clock_System_BusyWait_ms(4000);
    Event_Set(GOOD, "gOoD", 4);
    Event_Set(BAD, "bAd", 3);
    Event_Set(BAD, NULL, 0);

    Chip_Clock_System_BusyWait_ms(4000);
    Event_Set(GOOD, "GOOD", 4);

    /* Retrieving one dataset by index. */
    sCallCount = 0;
    Event_GetByIndex(0, 0, 0xAB);
    ASSERT(sCallCount == 1);

    /* Retrieving a few datasets by timestamp. */
    sCallCount = 0;
    sTimestamp += 4;
    Event_GetByTime(sTimestamp - 1, sTimestamp + 1, 0xCD);
    ASSERT(sCallCount == 3);

    /* Retrieving a few datasets by tag. */
    sCallCount = 0;
    Event_GetByTag(GOOD, 0xEF);
    ASSERT(sCallCount == 3);
//! [event_mod_example_1]
}
