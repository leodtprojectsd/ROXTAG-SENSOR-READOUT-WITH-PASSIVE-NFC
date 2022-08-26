# Copyright 2016,2020 NXP
# This software is owned or controlled by NXP and may only be used strictly
# in accordance with the applicable license terms.  By expressly accepting
# such terms or by downloading, installing, activating and/or otherwise using
# the software, you are agreeing that you have read, and that you agree to
# comply with and are bound by, such license terms.  If you do not agree to
# be bound by the applicable license terms, then you may not retain, install,
# activate or otherwise use the software.

import struct

# Transfer state
TRANSFER_FAILED = -1
TRANSFER_SUCCESSFUL = 0
TRANSFER_ONGOING = 1

# Max chunk size          NDEF     Mime Type MSG header
MAX_CHUNK_SIZE = 512 - (8 + 4 + 1) - 6 - 3 - 4


class Protocol(object):
    # MSG ID's
    _MSG_OUTGOING_PACKAGE = 0x48  # < Incremental data package */
    _MSG_LAST_OUTGOING_PACKAGE = 0x49  # < Last incremental data package including the binaryCrc

    _MSG_TAG_OUTGOING = 0
    _MSG_TAG_INCOMING = 1

    # Response ID's
    _RESP_DOWNLOAD_COMPLETE = 1  # < No error was found.
    _RESP_DOWNLOAD_ONGOING = 2  # < Ongoing Download.
    _RESP_DOWNLOAD_FAILED_IMAGE_TOO_BIG = 3  # < The image did not fit in user flash
    _RESP_DOWNLOAD_FAILED_FILE_INVALID_CHUNK_SIZE = 4  # < The given chunk size does not match with the total CMD size
    _RESP_DOWNLOAD_FAILED_FILE_CRC_MISMATCH = 5  # < The calculated CRC doesn't match with the received CRC
    _RESP_DOWNLOAD_COMMUNICATION_FAILED = 6  # < The communication failed
    _RESP_MSG_ERR_UNKNOWN_COMMAND = 0x10007  # < No suitable command handler could be found for this id
    _RESP_MSG_ERR_INVALID_COMMAND_SIZE = 0x1000D  # < A number of parameters are lacking or were given in excess.
    _RESP_MSG_ERR_INVALID_PARAMETER = 0x1000E  # < At least one parameter was missing or had an invalid value.

    # Expected version numbers
    _VERSION_SW_MAJOR = 1
    _VERSION_SW_MINOR = 2
    _VERSION_API_MAJOR = 4
    _VERSION_API_MINOR = 0

    @staticmethod
    def create_packet(chunk, file_crc):
        """
        This function will create an outgoing packet
        @par chunk : buffer containing the data chunk to be packed
        @return the packet to be send, None if failed
        """
        if len(chunk) > MAX_CHUNK_SIZE:
            print("ERROR: generateOutgoingPacket failed, chunk size too big")
            return None
        if file_crc is None:
            # its an incremental packet (not last one)
            packet = struct.pack('<BBH{}B'.format(len(chunk)), Protocol._MSG_OUTGOING_PACKAGE,
                                 Protocol._MSG_TAG_OUTGOING, len(chunk), *[c for c in chunk])
        else:
            packet = struct.pack('<BBH{}B'.format(len(chunk)), Protocol._MSG_LAST_OUTGOING_PACKAGE,
                                 Protocol._MSG_TAG_OUTGOING, len(chunk), *[c for c in chunk])
            packet += struct.pack('<H', file_crc)
        return packet

    @staticmethod
    def check_response(packet):
        """
        This function will validate a response packet
        @par packet : buffer containing the response packet
        @return
            - TRANSFER_FAILED if Download failed
            - TRANSFER_SUCCESSFUL if Download succeeded
            - TRANSFER_ONGOING if Download ongoing
            - None if packet was invalid
        """
        if len(packet) != struct.calcsize('=BBI'):
            return None
        result = TRANSFER_FAILED

        error_message = {Protocol._RESP_DOWNLOAD_FAILED_IMAGE_TOO_BIG: 'image does not fit in flash',
                         Protocol._RESP_DOWNLOAD_FAILED_FILE_INVALID_CHUNK_SIZE: 'invalid chunk size',
                         Protocol._RESP_DOWNLOAD_FAILED_FILE_CRC_MISMATCH: 'crc mismatch',
                         Protocol._RESP_DOWNLOAD_COMMUNICATION_FAILED: 'communication failed',
                         Protocol._RESP_MSG_ERR_INVALID_COMMAND_SIZE: 'invalid command size',
                         Protocol._RESP_MSG_ERR_INVALID_PARAMETER: 'invalid parameter'}

        try:
            if type(packet) == str:
                packet = bytearray(packet, encoding='utf_8', errors='strict')
            (msgId, tag, response) = struct.unpack('=BBI', packet)
            if tag is not Protocol._MSG_TAG_INCOMING:
                # The package is not a response.
                return None
            if response in error_message:
                print('ERROR: Download failed, {}'.format(error_message[response]))
                result = TRANSFER_FAILED

            elif (msgId, response) == (Protocol._MSG_OUTGOING_PACKAGE, Protocol._RESP_DOWNLOAD_ONGOING):
                result = TRANSFER_ONGOING

            elif (msgId, response) == (Protocol._MSG_LAST_OUTGOING_PACKAGE, Protocol._RESP_DOWNLOAD_COMPLETE):
                result = TRANSFER_SUCCESSFUL
            else:
                print('ERROR: invalid msgId/response combination : {}/{}'.format(msgId, response))

        except struct.error:
            print('Protocol ERROR, incoming response packet invalid.')
        return result

    @staticmethod
    def check_version(packet):
        """
        This function will validate a version packet
        @par packet : buffer containing the version packet
        @return True/False indicating packet was valid/invalid
        """
        if packet:
            if type(packet) == str:
                packet = bytearray(packet, encoding='utf_8', errors='strict')
            try:
                (_, tag, _, swMajorVersion, swMinorVersion, _, _, _) = struct.unpack('<BBHHHHHI', packet)
                print('Firmware version {}.{}'.format(swMajorVersion, swMinorVersion))
                ok = (Protocol._VERSION_SW_MAJOR, Protocol._VERSION_SW_MINOR) == (swMajorVersion, swMinorVersion)
            except (TypeError, struct.error):
                print('Protocol ERROR, incoming version packet invalid.')
                ok = False
        else:
            print('Protocol ERROR, invalid packet received.')
            ok = False
        return ok
