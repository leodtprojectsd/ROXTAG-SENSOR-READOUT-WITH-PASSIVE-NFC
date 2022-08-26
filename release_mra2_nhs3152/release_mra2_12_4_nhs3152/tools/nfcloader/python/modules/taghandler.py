# Copyright 2016,2020 NXP
# This software is owned or controlled by NXP and may only be used strictly
# in accordance with the applicable license terms.  By expressly accepting
# such terms or by downloading, installing, activating and/or otherwise using
# the software, you are agreeing that you have read, and that you agree to
# comply with and are bound by, such license terms.  If you do not agree to
# be bound by the applicable license terms, then you may not retain, install,
# activate or otherwise use the software.

import abc


class TagHandler(object):
    """
    Abstract class holding information on tag handling
    This class is to be used as base class for multiple implementations using different libraries
    This class provides standardised api for these implementations.
    """
    __metaclass__ = abc.ABCMeta

    def __init__(self, tagreader):
        """
        creates an instance of TagHandler.
        This 'constructor blocks till a target is tu'
        """
        self.tagreader = tagreader

    def read(self):
        """
        This function reads the first record in the tag (NDEF message)
        @return string containing the record payload
        """
        raise NotImplementedError("Subclass must implement abstract method")

    def write(self, packet):
        """
        This function writes a single MimeType record NDEF message including the packet
        @param packet : The packet to be send to the tag
        """
        raise NotImplementedError("Subclass must implement abstract method")
