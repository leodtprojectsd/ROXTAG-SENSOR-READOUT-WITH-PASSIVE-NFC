/**
 * @defgroup APP_NSS_NFCCERTIFICATION nfccertification: firmware allowing the NFC certification of any NHS31xx based tag.
 *  @ingroup APPS_NSS
 *
 *  @par Introduction
 *  All NHS31xx ICs are pre-certified: the ICs underwent the full certification process mounted on the NHS3100
 *  temperature logger demo PCB, whose schematics and layout can be found in the SDK. The ICs were flashed using this
 *  application firmware.
 *  More information about the certification and its process can be found here:
 *     - https://nfc-forum.org/our-work/compliance/certification-program/
 *     - https://nfc-forum.org/wp-content/uploads/2017/08/NXP_NHS3100_Ref_Des_CertID_58516.pdf
 *     - https://nfc-forum.org/wp-content/uploads/2017/08/NXP_NHS3152_CertID_58524.pdf
 *
 *  The firmware is listed here as a means to allow you to qualify your product, in the same way, minimizing the
 *  efforts needed to be undertaken.
 *  This application can also be used whenever the NFC controller or the antenna range is to be tested.
 *
 *  @par Firmware
 *  All the firmware does is decouple the-the NFC memory from the ARM Cortex M0+ by deactivating the shared memory
 *  interface and corresponding registers. This gives an external reader full access to the RFID/NFC EEPROM.
 *
 *  @par Operation
 *  The NHS31xx IC will deactivate the shared memory interface and then go into an endless loop.
 *  An external reader can then use the IC as a simple Ultralight EV1 tag. Any read or write will be a read or write
 *  into the NFC EEPROM. Communication with the ARM Cortex M0+ is no longer possible; nor is it possible to access the
 *  ARM Flash and ARM EEPROM non-volatile memories.
 */
