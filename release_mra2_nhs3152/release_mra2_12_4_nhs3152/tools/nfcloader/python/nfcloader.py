# Copyright 2016,2020 NXP
# This software is owned or controlled by NXP and may only be used strictly
# in accordance with the applicable license terms.  By expressly accepting
# such terms or by downloading, installing, activating and/or otherwise using
# the software, you are agreeing that you have read, and that you agree to
# comply with and are bound by, such license terms.  If you do not agree to
# be bound by the applicable license terms, then you may not retain, install,
# activate or otherwise use the software.

"""
The NHS31xx one-time NFC program downloader is the default firmware image flashed by NXP in all NHS31xx ICs during
production. This python3 script is one example how the host side can be implemented to communicate with that firmware.

Using this script, you can download over the NFC interface your final firmware image. This can only be done once: upon
successful completion, the default firmware image will be at least partially overwritten with your firmware image, and
your firmware image will be executed after a reset.

More information about setup and usage can be found in the accompanying markdown file.
"""

import os
import argparse
import time

from modules.protocol import *
from modules.crc16 import Crc16


def nfcLoader(binary, driver, tag_reader=None):
    try:
        # Call the right constructor. This will be a blocking call until a tag is found.
        if driver == 'nfcpy':
            from modules.taghandler_nfcpy import TagHandler_nfcpy
            tag_handle = TagHandler_nfcpy(tag_reader)
        elif driver == 'pyscard':
            from modules.taghandler_pyscard import TagHandler_pyscard
            tag_handle = TagHandler_pyscard(tag_reader)
        else:
            raise Exception('Unsupported driver package requested')

        # Give the tag 1 second to initiate the download (by writing the version)
        target_ready = False
        start_version_check = time.time()
        while not target_ready and (time.time() < start_version_check + 1):
            if Protocol.check_version(tag_handle.read()):
                target_ready = True
                break
            if not target_ready:
                time.sleep(0.15)
        if not target_ready:
            raise Exception('Correct application version of tag not found within time limit')
        print('Tag present, application version OK')

        start_time = time.time()
        transfer = TRANSFER_ONGOING
        crc_handler = Crc16()
        file_size = os.fstat(binary.fileno()).st_size

        file_crc = None
        while transfer == TRANSFER_ONGOING:
            chunk = binary.read(MAX_CHUNK_SIZE)
            file_crc = crc_handler.calc(chunk)  # Update the running crc
            if len(chunk) > MAX_CHUNK_SIZE - 2:
                # this is not the last chunk, do not send the crc
                print('[{: >5.2f}] Sending {} bytes of data ...'.format(time.time() - start_time, len(chunk)))
                file_crc = None
            else:
                # this is the last chunk, also send the crc
                print('[{: >5.2f}] Sending the last {} bytes of data ...'.format(time.time() - start_time, len(chunk)))

            tag_handle.write(Protocol.create_packet(chunk, file_crc))
            print('        ... data sent. Waiting for response ...')

            # give the tag ample time to acknowledge the data chunk
            transfer = None
            start_response_check = time.time()
            while transfer is None and (time.time() < start_response_check + 1):
                resPack = tag_handle.read()
                if resPack:
                    transfer = Protocol.check_response(resPack)
                    if transfer:
                        print('        ... response received.')
        if transfer != TRANSFER_SUCCESSFUL:
            s = 'Transmission of data, or reception of response failed.'
            if file_crc:
                s += ' This error occurred during the critical point:' \
                     ' the one-time NFC program downloader firmware may already have been erased.'
            else:
                s += ' Reset the IC and try again.'
            raise Exception(s)

        print('Download successful')
        end_time = time.time()
        print('Transmitted (bytes): {}'.format(file_size))
        print('Speed (bytes/sec): {: >5.2f}'.format(file_size / (end_time - start_time)))
        result = 0

    except KeyboardInterrupt:
        print('ERROR: download aborted on user request.')
        result = -2
    except Exception as e:
        print('ERROR: download failed, {}'.format(e))
        result = -1
    finally:
        binary.close()
    return result


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('file', metavar='file', type=argparse.FileType('rb'), help='The binary file to be loaded into the target')
    parser.add_argument('-d', '--driver', help='Driver to be used for tag handling', choices=['nfcpy', 'pyscard'], default='pyscard')
    parser.add_argument('-t', '--tagreader', help='tag reader to connect to', default=None)
    parser.formatter_class = argparse.ArgumentDefaultsHelpFormatter
    args = parser.parse_args()

    print('NHS31xx one-time NFC program downloader')
    exit(nfcLoader(args.file, args.driver, args.tagreader))
