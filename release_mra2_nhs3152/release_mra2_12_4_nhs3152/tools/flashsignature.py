# Copyright 2017-2020 NXP
# This software is owned or controlled by NXP and may only be used strictly
# in accordance with the applicable license terms.  By expressly accepting
# such terms or by downloading, installing, activating and/or otherwise using
# the software, you are agreeing that you have read, and that you agree to
# comply with and are bound by, such license terms.  If you do not agree to
# be bound by the applicable license terms, then you may not retain, install,
# activate or otherwise use the software.

"""
A Python3 script to generate the corresponding FLASH signature of a binary file.
The flash signature is printed out in hexadecimal format on standard output.
Note:
    Use this python script to prepare a new order entry form (OEF2) submission. For more information, see
    application note AN12251: NHS3100W8 customer firmware flashing.
"""

from __future__ import print_function
import argparse
from functools import partial


image_file_help = '''
    relative or absolute path to a binary file.
    Do not provide a file in object file format - extension .elf or .axf - or an Intel hex format - extension .hex. 
    The build output by gcc can be converted to a binary format - extension .bin - using the GNU object copy tool for
    ARM Embedded Processors arm-none-eabi-objcopy with '-O binary' as argument. The binary file must have been built for
    an NHS31xx IC - and thus a.o. have a size in bytes which is a multiple of 4.
'''

store_true_help = '''
    only print out the FLASH signature and nothing else. Useful for scripting. Use this option and redirect the output
    to a file with extension .signature to create a correct flash signature file.
'''

exception_help = '''
    Check your file argument and try again. Did you convert the file to the correct file format? 
'''


def generateFlashSignature(file):
    def rotr(num, bits):
        num &= (2**bits-1)
        bit = num & 1
        num >>= 1
        if bit:
            num |= (1 << (bits-1))
        return num

    signature = 0x0000000

    for word in iter(partial(file.read, 4), b''):
        word_i = word[0] + (word[1] << 8) + (word[2] << 16) + (word[3] << 24)
        for tap in [10, 30, 31]:
            signature ^= ((signature >> tap) & 1)
        signature = rotr(signature, 32)
        signature ^= word_i
    return signature


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('f', metavar='image_file', type=argparse.FileType('rb'), help=image_file_help)
    parser.add_argument('-z', action='store_true', help=store_true_help)
    args = parser.parse_args()

    try:
        s = hex(generateFlashSignature(args.f))
        exit_value = 0
    except Exception as e:
        print('Script execution error: "{}". {}'.format(e, exception_help.strip()))
        s = 'could not be calculated.'
        exit_value = 1
    if args.z:
        print(s, end='')
    else:
        print('FLASH signature of {}: {}'.format(args.f.name, s))
    
    exit(exit_value)
