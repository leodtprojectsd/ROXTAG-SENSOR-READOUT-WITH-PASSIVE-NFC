/*
 * Copyright 2016-2017,2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"
#include "msg/msg.h"
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wincompatible-pointer-types"
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
#pragma GCC diagnostic ignored "-Wunused-variable"

#define ResponseCb doc_msg_example_cb

//! [msg_mod_cmd_handler]
static uint32_t Handler77(uint8_t msgId, int payloadLen, const uint8_t* pPayload);
MSG_CMD_HANDLER_T appCmdHandler[MSG_APP_HANDLERS_COUNT] = { {0x77, Handler77},
                                                            /* Additional commands can be added here. */
                                                          };
//! [msg_mod_cmd_handler]

static uint32_t Handler77(uint8_t msgId, int payloadLen, const uint8_t* pPayload)
{
//! [msg_mod_handle]
    uint8_t response[1];
    response[0] = 7;
    Msg_AddResponse(0x77, 1, response);
    return MSG_OK;
//! [msg_mod_handle]
}

bool doc_msg_example_cb(int responseLength, const uint8_t* pResponseData)
{
//! [msg_mod_responseCb]
    /* Send the bytes over I2C, encapsulate it in an NDEF MIME message, or ... */
    return true;
//! [msg_mod_responseCb]
}

void doc_msg_example_init(void)
{
//! [msg_mod_init]
    Msg_Init();
    Msg_SetResponseCb(ResponseCb);
//! [msg_mod_init]
}

void doc_msg_example(void)
{
//! [msg_mod_rxdata]
    uint8_t buffer[2] = {};
    uint8_t length = 2;
    /* Retrieve the bytes that were sent to the NHS over I2C, NFC, or ... */
    buffer[0] = 0x77; /* message id */
    buffer[1] = 0; /* directionality byte */
    Msg_HandleCommand(length, buffer);
//! [msg_mod_rxdata]
}

#undef ResponseCb
#pragma GCC diagnostic pop
