# Copyright 2016,2018,2020 NXP
# This software is owned or controlled by NXP and may only be used strictly
# in accordance with the applicable license terms.  By expressly accepting
# such terms or by downloading, installing, activating and/or otherwise using
# the software, you are agreeing that you have read, and that you agree to
# comply with and are bound by, such license terms.  If you do not agree to
# be bound by the applicable license terms, then you may not retain, install,
# activate or otherwise use the software.

import sys
import time

from modules import taghandler
try:
    import nfc
    import ndef
except ImportError:
    print('You are missing nfcpy, this is a driver package used for interaction with an NFC reader. \n'
          'Please refer to the readme file for more info on installing it.')
    sys.exit(-1)


class TagHandler_nfcpy(taghandler.TagHandler):
    def __init__(self, tagreader):
        if not tagreader:
            tagreader = 'usb'
        super(self.__class__, self).__init__(tagreader)
        self.MIME_TYPE = 'N/P'

        self.clf = None
        try:
            self.clf = nfc.ContactlessFrontend(self.tagreader)
        except IOError:
            raise Exception('Tag reader via {} not found.'.format(self.tagreader))
        print('Now accessing tag reader {}.'.format(self.clf.device))

        def on_connect(tag):
            print('TAG connected: {}'.format(tag))
            return False

        print('Searching for a tag')
        self.nfc_tag = None
        while not self.nfc_tag:
            try:
                self.nfc_tag = self.clf.connect(rdwr={'on-connect': on_connect})
                if not self.nfc_tag.ndef:
                    print('Tag does not contain an NDEF message. Place a correct tag.')
                    self.nfc_tag = None
                    time.sleep(1)
            except IOError:
                print('No tag found. Place a tag.')
                time.sleep(1)

    def __del__(self):
        if self.clf:
            self.clf.close()

    def read(self):
        packet = None
        if self.nfc_tag.ndef:
            _ = self.nfc_tag.ndef.has_changed  # Force a read from the target
        if self.nfc_tag.ndef:
            records = self.nfc_tag.ndef.records
            if records:
                packet = records[0].data
                # print(' - nfcpy read', ' '.join(format(d, '02X') for d in packet))
        return packet

    def write(self, packet):
        # print(' - nfcpy write', ' '.join(format(d, '02X') for d in packet))
        self.nfc_tag.ndef.records = [ndef.Record(self.MIME_TYPE, '', packet)]
