/*
 * Copyright 2016,2018-2019 NXP
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

/** @defgroup APP_NSS_NFCPROGRAMLOADER_MSGHANDLER_PROTOCOL NFC Program loader app.spec. messages
 *  @ingroup APP_NSS_NFCPROGRAMLOADER
 *  This file describes the protocol used for the Message Handler Module in the NFC program loader application (closed).
 *
 *  -# (NHS31xx) Once the NHS31xx is started (either by a resetN or a by a NFC field) the downloader is initialized, and
 *      a @ref MSG_RESPONSE_GETVERSION_T response is placed in NFC memory.
 *  -# (HOST) From that moment on, a HOST (tag reader) can start sending @ref CMD_INCOMING_PACKAGE_T commands containing
 *      data chunks. Every incremental message has an id equal to #MSG_INCOMING_PACKAGE.
 *      The NHS31xx expects a binary file to be downloaded from lower to higher addresses (starting from address 0x0).
 *  -# (NHS31xx) This command will be answered/acknowledged as fast as possible, this is done via a
 *      @ref MSG_RESPONSE_RESULTONLY_T response. The @c result can be a @ref MSG_ERR_T or a
 *      @ref DOWNLOAD_REPORT_MSG_ERR_T
 *  -# (HOST) as long as the result is #DOWNLOAD_ONGOING, the host can send new data chunks.
 *  -# (HOST) the very last package send to NHS31xx is slightly different from the incremental once, the header looks
 *      the same accept for the message id which is equal to #MSG_LAST_INCOMING_PACKAGE. In this package, the data chunk
 *      is followed by the file CRC. The new program download will only be finalized when the in
 *      target calculated CRC matches the file CRC.
 *  .
 *
 * Following message flow describes a successful download cycle:
 * @msc
 *      hscale="1.6", wordwraparcs="true";
 *      A [label="NHS31xx"],
 *           B [label="HOST (PC/raspberry pi)"];
 *      |||;
 *           A => B         [label="Initial Message \n GetVersion response", URL="@ref MSG_RESPONSE_GETVERSION_T"];
 *           |||;
 *           A <= B    [label="CMD incoming package 1/X \n MSG_INCOMING_PACKAGE", URL="@ref CMD_INCOMING_PACKAGE_T"];
 *           |||;
 *           A => B         [label="Acknowledge incoming package \n MSG_INCOMING_PACKAGE \n result DOWNLOAD_ONGOING", URL="@ref MSG_RESPONSE_RESULTONLY_T"];
 *           A->A [label="Process data of package 1/x"];
 *           |||;
 *           A <= B    [label="CMD incoming package 2/X \n MSG_INCOMING_PACKAGE", URL="@ref CMD_INCOMING_PACKAGE_T"];
 *           |||;
 *           A => B         [label="Acknowledge incoming package \n MSG_INCOMING_PACKAGE \n result DOWNLOAD_ONGOING", URL="@ref MSG_RESPONSE_RESULTONLY_T"];
 *           A->A [label="Process data of package 2/x"];
 *      ...;
 *      ...;
 *      ... [label="...transfer all packages till X-1..."];
 *      ...;
 *      ...;
 *           A <= B    [label="CMD incoming package X/X (including file CRC) \n MSG_LAST_INCOMING_PACKAGE", URL="@ref CMD_INCOMING_PACKAGE_T"];
 *           A->A [label="Process data of last package"];
 *           |||;
 *           A => B         [label="Acknowledge incoming package \n MSG_LAST_INCOMING_PACKAGE \n result DOWNLOAD_COMPLETE_OK", URL="@ref MSG_RESPONSE_RESULTONLY_T"];
 *      @endmsc
 *
 * @note Mind that, if for some reason an error occurs (protocol error/download error) the response will contain the
 *  corresponding result (@ref MSG_ERR_T or @ref DOWNLOAD_REPORT_MSG_ERR_T). If that happens,
 *  the NHS31xx needs to go through a reset cycle. Either by resetN or by re-providing the NFC field.
 *  iow, the complete download cycle has to be re-done.
 * @{
 */

/** Mime type to be used in the data transfer */
#define MIME_TYPE "N/P"

/** Size of mime type used in the data transfer */
#define MIME_TYPE_LENGTH 3

/** Maximum size of data chunk to be send by the HOST
 * Using this size will result in optimal use of shared memory */
#define MAX_CHUNK_SIZE NFC_SHARED_MEM_BYTE_SIZE - NDEFT2T_MSG_OVERHEAD(false, NDEFT2T_GetMimeRecordOverhead(false, MIME_TYPE_LENGTH) + sizeof(CMD_INCOMING_PACKAGE_T))

/** Lists all possible download errors codes that may be returned. Extends #MSG_ERR_T*/
typedef enum DOWNLOAD_REPORT_MSG_ERR {
    /* 0 is reserved by MSG_ERR_T */
    DOWNLOAD_COMPLETE_OK = 1, /**< @c 0x01 @n No error was found. */
    DOWNLOAD_ONGOING = 2, /**< @c 0x02 @n Ongoing Download. */
    DOWNLOAD_FAILED_IMAGE_TOO_BIG = 3, /**< @c 0x03 @n The image did not fit in user flash */
    DOWNLOAD_FAILED_FILE_INVALID_CHUNKSIZE = 4, /**< @c 0x04 @n The given chunk size does not match with the total CMD size */
    DOWNLOAD_FAILED_FILE_CRC_MISMATCH = 5, /**< @c 0x05 @n The calculated CRC doesn't match with the received CRC  */
    DOWNLOAD_COMMUNICATION_FAILED = 6, /**< @c 0x06 @n General ERROR, i.e. non valid command ID  */

    DOWNLOAD_REPORT_MSG_ERR_LAST_USED = DOWNLOAD_COMMUNICATION_FAILED,
} DOWNLOAD_REPORT_MSG_ERR_T;

/** Supported application commands */
typedef enum MSGHANDLER_MSG_ID_S {
    MSG_INCOMING_PACKAGE = 0x48, /**< @c 0x48 @n Incremental data package */
    MSG_LAST_INCOMING_PACKAGE = 0x49, /**< @c 0x49 @n Last incremental data package including the binary CRC */
} MSGHANDLER_MSG_ID_T;

#pragma pack(push, 1)
/**
 * Structure defining the incoming packages
 * This 'header' is followed by the data chunk with size defined in @c chunkSize.
 * If @c id is #MSG_LAST_INCOMING_PACKAGE this 'header' + data chunk is followed by the 16bits binary CRC.
 * @see MSG_INCOMING_PACKAGE
 * @see MSG_LAST_INCOMING_PACKAGE
 */
typedef struct CMD_INCOMING_PACKAGE_S {
    /**
     * This field holds the number of valid data bytes which will follow after this header.
     * @note This value is limited by #MAX_CHUNK_SIZE.
     * @note If @c id is #MSG_LAST_INCOMING_PACKAGE chunkSize does not include the CRC size.
     */
    uint16_t chunkSize;
} CMD_INCOMING_PACKAGE_T;
#pragma pack(pop)

#endif /** @} */
