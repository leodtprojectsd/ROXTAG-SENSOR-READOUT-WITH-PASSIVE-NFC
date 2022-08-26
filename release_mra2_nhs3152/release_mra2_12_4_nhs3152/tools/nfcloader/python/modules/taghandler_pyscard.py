# Copyright 2016,2018-2020 NXP
# This software is owned or controlled by NXP and may only be used strictly
# in accordance with the applicable license terms.  By expressly accepting
# such terms or by downloading, installing, activating and/or otherwise using
# the software, you are agreeing that you have read, and that you agree to
# comply with and are bound by, such license terms.  If you do not agree to
# be bound by the applicable license terms, then you may not retain, install,
# activate or otherwise use the software.

import sys
import struct
from modules import taghandler
try:
    import smartcard
    import smartcard.scard
    from smartcard.System import readers
except ImportError:
    print('You are missing pyscard, this is a driver package used for interaction with a smartcard reader. \n'
          'Please refer to the readme file for more info on installing it.')
    sys.exit(-1)


class TagHandler_pyscard(taghandler.TagHandler):

    def __init__(self, tagreader=None):
        super(self.__class__, self).__init__(tagreader)
        self.MIME_TYPE = [ord(c) for c in 'N/P']
        self.__SUCCESS = 144  # Value returned by connection.transmit if transmit was successful
        self.__START_PAGE = 0x06  # page where to expect an NDEF message
        self.connection = None
        rs = readers()
        reader = None
        if not rs:
            raise Exception('No supported readers found.')
        else:
            print('Available readers: {}'.format(rs))
        if tagreader is None:
            reader = rs[0]
        else:
            for r in rs:
                if self.tagreader.lower() in str(r).lower():
                    reader = r
                    break
        if reader is None:
            raise Exception('Reader "{}" not found.'.format(self.tagreader))

        print('Now accessing tag reader {}'.format(str(reader)))
        print('Searching for a tag')
        self.connection = reader.createConnection()
        while 1:
            try:
                self.connection.connect()
                print('TAG connected')
                break
            except smartcard.Exceptions.NoCardException:
                continue

    def __del__(self):
        if self.connection:
            self.connection.disconnect()

    def __writeWord(self, page, word):
        """
            This function writes a 4 Bytes word to a page in the tag
            @par page : page offset, starting at 0, 0 will end up in page 6 (start of NDEF message)
            @par word : the 4 bytes word to be put in the page specified by @c page
        """
        # Standard command format (read command)
        command = [0xFF, 0xD6, 0x00, 0x04, 0x04, 0x00, 0x01, 0x02, 0x03]
        
        # Update data block
        # 6 is our first page
        command[3] = page + 6   # NDEF starts at page 6
        command[5:] = word
        d, sw1, sw2 = self.connection.transmit(command)
        return sw1 == self.__SUCCESS

    def read(self):
        """
        This function tries to read a MIME record/message from a tag
        The function expects can parse a message up to 32 bytes in size (including ndef + record headers)
        @return
            - None if read/parsing failed
            - Else Mime payload
        """

        data = []
        read_cursor = self.__START_PAGE
        word_size = 0x4
        size_to_read = 0x20
        # first try to read 4 words (1 block) at once
        chunk_size = 0x10

        while size_to_read > 0:
            # Read the Message in the tag
            d, sw1, sw2 = self.connection.transmit([0xFF, 0xB0, 0x00, read_cursor, chunk_size])
            if sw1 != self.__SUCCESS:
                return None
            if len(d) == word_size:
                # reader only reads words (no 16bytes blocks)
                chunk_size = word_size
            data += d
            size_to_read -= chunk_size
            read_cursor += chunk_size // word_size

        # print(' - pyscard read', ' '.join(format(d, '02X') for d in data))
        # Parse the message
        #      NDEF Type        NDEF LENGTH         MIME TYPE        TYPE + PAYLOAD LENGTH
        if data[0] == 0x03 and data[1] < 30 and data[2] == 0xD2 and data[3] + data[4] < 27:
            # Return the MIME payload in string format
            return ''.join(chr(c) for c in data[(5+data[3]):(5+data[3]+data[4])])
        else:
            return None

    def write(self, packet):
        """
        This function writes a single MimeType record NDEF message including the packet
        @param packet : The packet to be send to the tag
        """
        # print(' - pyscard write', ' '.join(format(c, '02X') for c in packet))

        # Calculate NDEF length
        #          MIME TyL     Type        PlL   Payload
        ndef_length = 1 + 1 + len(self.MIME_TYPE) + 1 + len(packet)
        if len(packet) > 0xFF:
            ndef_length += 3

        out = [0x03, 0x00]
        if ndef_length > 0xFE:
            # 3 bytes message size iso 1
            out += [0x00, 0x00]
        if len(packet) > 0xFF:
            # long record
            out += [0xC2, len(self.MIME_TYPE)]
            out += list(struct.unpack('4B', struct.pack('>I', len(packet))))
        else:
            out += [0xD2, len(self.MIME_TYPE), len(packet)]
        out += [c for c in self.MIME_TYPE]
        out += [c for c in packet]

        # Generate first page including size bytes
        firstPage = [out[0]]
        if ndef_length > 0xFE:
            firstPage += [0xFF] + list(struct.unpack('2B', struct.pack('>H', ndef_length)))
        else:
            firstPage += [ndef_length] + out[2:4]

        # make sure out length is multiple of 4
        if len(out) % 4:
            out += [0x0 for _ in range(4-(len(out) % 4))]

        # Send message word by word
        wordSize = 4
        for page, index in enumerate(range(0, len(out), wordSize)):
            if not self.__writeWord(page, out[index:index+wordSize]):
                raise IOError('ERROR while word writing page {}.'.format(page))

        # write the first page to finish the message
        if not self.__writeWord(0, firstPage):
            raise IOError('ERROR while word writing page {}.'.format(0))
