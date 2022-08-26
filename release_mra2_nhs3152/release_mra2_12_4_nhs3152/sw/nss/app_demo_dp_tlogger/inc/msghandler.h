/*
 * Copyright 2015-2018 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __MSGHANDLER_H_
#define __MSGHANDLER_H_

/** @addtogroup APP_DEMO_TLOGGER_MSGHANDLER Message Handler block
 * @ingroup APP_DEMO_TLOGGER
 * The Temperature Logger Message Handler is responsible for handling the communication with the host
 * (tag reader/smartphone).
 *  -# It makes use of:
 *      - @ref NFC_NSS as its communication channel.
 *      - @ref TIMER_NSS (Timer 0) to implement a Host-Timeout detection mechanism.
 *      - @ref MODS_NSS_MSG module to implement a command/response mechanism.
 *      - @ref MODS_NSS_NDEFT2T to generate/parse NDEF formatted messages.
 *      .
 *  -# The supported command ID's are described by #APP_MSG_ID_T enum
 *  -# The specifics of the protocol are described in the documentation section of the @ref MODS_NSS_MSG module
 *  -# The content of the commands and responses defined by the Temperature Logger demo application
 *      are described by @ref APP_DEMO_TLOGGER_MSGHANDLER_PROTOCOL.
 *  -# It is also responsible for sending and receiving the data to the underlying physical communication channel
 *      (NFC interface).
 *  .
 * @{
 */

#include <stdint.h>
#include <stdbool.h>

/* ------------------------------------------------------------------------- */

/**
 * Initializes the messaging part of the application; initializes the msg mod;
 * Ensures an initial ndef message is loaded in the NFC shared memory.
 * @param reuseKeys @c True when previously set keys - if any - are to be reused; @c false otherwise.
 */
void AppMsgInit(bool reuseKeys);

/**
 * Wrapper round #Msg_HandleCommand
 * @pre AppMsgInit must have been called beforehand
 * @param cmdLength : The size in bytes in @c cmdData
 * @param cmdData : Pointer to the array containing the raw command bytes.
 */
void AppMsgHandleCommand(int cmdLength, const uint8_t* cmdData);

#endif /** @} */
