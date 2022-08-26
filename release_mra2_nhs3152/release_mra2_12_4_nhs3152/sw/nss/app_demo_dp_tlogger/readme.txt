/**
 * @defgroup APP_DEMO_TLOGGER tlogger: Temperature Logger demo application firmware
 * @ingroup APPS_NSS
 * The Temperature Logger demo application firmware demonstrates the value proposition around the Temperature Logger use
 * case. This application, together with a label or a demo PCB and a matching tag reader application on Android, iOS,
 * macOS or Windows, can serve as a starting point for application and APP developers to develop a full-blown solution 
 * for NFC-enabled temperature logging.
 *
 * @par Introduction
 *  This demo application is designed to run on an NHS31xx IC mounted on a NHS3100 temperature logger Demo PCB - which
 *  can be ordered directly at nxp.com/NTAGSMARTSENSOR. @n
 *  The application firmware is built on top of the SDK offering: it uses the APIs exposed by the chip and board
 *  libraries, and it makes use of the available modules as much as possible.
 *
 * @par Getting started and how to operate
 *  With the firmware image loaded in an NHS31xx IC, all you need next is any NFC reader. Best demo results are achieved
 *  when using an NFC-enabled Android smartphone, or an iPhone 7 or higher. In that case the <em>NHS3100 Temperature
 *  Logger</em> can be used, which is available in the Google Play Store or the Apple App store. All configuration and
 *  data retrieval can be done via the APP (note that iOS only supports NDEF reading, nor writing, so setting a
 *  configuration requires either an Android phone or the use of the macOS application).
 *  Without the APP, on an Android smartphone, only status information can be retrieved in the form of a textual message
 *  that pops up on the screen. This textual retrieval is supported out of the box on Android, and can be done with any
 *  freely available generic NFC APP on all other platforms.
 *
 *  It is also possible to hook up an NFC reader via a USB port to your PC, and to use a Python script or a full blown
 *  application - check the NHS3100 Temperature Logger application for macOS - to communicate with the tlogger firmware
 *  image.
 *
 * @par ARM application overview
 *  - The execution flow of the application is described in the following diagram:
 *      @b TODO
 *  - The block decomposition of the application is clarified in the following block diagram:
 *      @b TODO
 *  - To enable communication of data between the tag - containing the NHS3100 IC - and a tag reader - whether it is a
 *      smartphone or a PC - the NFC controller is used to read and write NDEF messages containing MIME records. The
 *      full communication protocol is outlined below.
 *  .
 * 
 * @anchor app_demo_tlogger_communication_protocol_anchor
 * @par Communication protocol
 *  - For maximum compatibility, the use of NDEF messages is required: iOS 11 running on an iPhone 7 or higher only
 *      allows reading of NDEF messages. The application code relies on the NDEFT2T module to ensure that:
 *      - all data is correctly encapsulated in a MIME record inside an NDEFT2T message @n
 *          when sending data from the tag to the tag reader; and vice versa,
 *      - to extract the payload bytes in a MIME record inside an NDEFT2T message @n
 *          when receiving data from the tag reader.
 *      .
 *  - The payload bytes in a MIME record are in turn formatted according to the rules stipulated by the <em>msg</em>
 *      module. Detailed information about these rules can be found with @ref MODS_NSS_MSG.
 *      The <em>msg</em> module allows for the inclusion of application specific messages, which the application uses to
 *      allow getting and retrieving the configuration details, and the gathered data (temperature measurements). The
 *      details about the application specific messages are captured in the protocol submodule
 *      @ref APP_DEMO_TLOGGER_MSGHANDLER_PROTOCOL.
 *  - The full communication flow from tag to tag reader is largely inline with the definitions and workflow outlined
 *      in the <em>msg</em> module:
 *      @n
 *      - (A,B) The tag reader sends a command, which is wrapped in a message containing a single MIME record,
 *      - (C) by writing into the NFC memory. The NFC controller notifies the ARM Cortex M0+,
 *      - (D) after which the application code reads out the command and extracts the MIME payload - using the NDEFT2T
 *          module - and feeds it to the <em>msg</em> module,
 *      - (E) which then calls the registered handler function. The end result of handling is the generation of a
 *          response,
 *      - (D) which is again wrapped in a message containing a single MIME record,
 *      - (C) and finally copied to the NFC memory - thereby overwriting the command that was originally written by the
 *          tag reader.
 *      - (B) The tag reader reads out the same NFC memory again, extracts the MIME payload,
 *      - (A) and interprets the response bytes to update his logs and GUI.
 *      .
 *      @n
 *      @msc
 *      hscale="1.6", wordwraparcs="true";
 *      A [label="A: tag reader\napplication code"],
 *           B [label="B: tag reader\nNFC communication module"],
 *                C [label="C: tag\nNFC memory"],
 *                     D [label="D: tag\nNDEFT2T module"],
 *                          E [label="E: tag\nmsg module"];
 *      |||;
 *      A => B                   [label="[msgId, dir, C, ... Z]\ncommand sequence of bytes", URL="@ref msg_anchor_protocol"];
 *           B => C              [label="NFC write: a command\nwrapped in an NDEFT2T message"];
 *                C >> D         [label="notification upon\nwrite completion"];
 *                C => D         [label="copy: a command\nwrapped in an NDEFT2T message"];
 *                     D => E    [label="[msgId, dir, C, ... Z]\ncommand sequence of bytes"];
 *                          E->E [label="Generate response"];
 *                     D <= E    [label="[msgId, dir, c, ... z]\nresponse sequence of bytes", URL="@ref msg_anchor_protocol"];
 *                C <= D         [label="copy: a response\nwrapped in an NDEFT2T message"];
 *           B <= C              [label="NFC read: a response\nwrapped in an NDEFT2T message"];
 *      A <= B                   [label="[msgId, dir, c, ... z]\nresponse sequence of bytes", URL="@ref msg_anchor_protocol"];
 *      |||;
 *      @endmsc
 *      @n
 *  - There are two deviations to the otherwise strict command - response way of communication. The first occurs at the
 *      startup of the IC: the tag will then create a multi-record message containing the responses to a few general
 *      commands, which the tag assumes any tag reader will want to know. For demo purposes, to demonstrate the
 *      Android OS is perfectly capable of decoding our standards compliant NDEFT2T messages, one or more text records
 *      are also included, which describe in English text form the current state of the IC and the temperature
 *      monitoring.
 *      The text records are ignored by the Android APP, but are used by the iOS APP to display the current status.
 *      The Android and iOS APPs currently both expect a few specific MIME records to be present: they do not look at
 *      the type of the NFC controller being connected to, but read out the initial message available in the NFC memory,
 *      and decide whether communication is possible, based on the responses with message id #MSG_ID_GETVERSION and
 *      #APP_MSG_ID_GETCONFIG (in that order) they can extract from that. Of course, all other MIME records present are
 *      also parsed and taken into account.@n
 *      Note that this approach also poses a risk: there is no guideline or standard enforcing a minimal delay between
 *      connecting with a tag and reading out the NFC memory. In theory, this means that a tag reader can read out
 *      whatever is present immediately after successfully establishing a connection. The time between detecting an NFC
 *      field - after which the IC will start up - and the copying of the initial multi-record message may be too short:
 *      the application firmware may be too late to provide the expected information, which causes the APP to conclude
 *      an alien tag is connected to, and communication is not possible. This is the reason why at startup, in
 *      #ResetISR, the system clock frequency is upped to 2MHz.
 *  - The second deviation is added to enable as much functionality as possible of the temperature logger demo for iOS
 *      users, where NFC writing is not possible. All NFC interrupts are handled by the NDEFT2T module: it can use it to
 *      - correctly write NDEF messages - even when spurious writes occur by the tag reader
 *      - correctly read NDEF messages - preventing simultaneous reading by the tag and writing by the tag writer which
 *          would otherwise lead to race conditions
 *      - correctly detect when the tag reader has finished reading the NDEF message that is available in the NFC
 *          memory.
 *      .
 *      Due to slightly different implementations of the tag readers' NFC stack libraries on different systems and OS
 *      versions, it is not possible to simultaneously and correctly take care for all three cases listed above.
 *      At startup, after writing the initial NDEF message. It enables an 'automatic mode' - using
 *      #NDEFT2T_EnableAutomaticMode - where the application uses the 'message read' notifications as a trigger to play
 *      a script: each 'message read' notification triggers the creation of a new command, generated in the application
 *      code in the @c GenerateNextAutomaticCommand, which serves as a replacement of a real NDEF message write of the
 *      tag reader.
 *      As long as the tag reader is not writing to the NFC memory, the tag assumes it knows which responses the tag
 *      reader wants to fetch from the tag, and continually updates the NFC memory with new responses. This approach
 *      has two advantages:
 *      - Using an iOS APP, all data measurements can be read out.
 *      - All data measurements can be read out slightly faster than by a continual command - response exchanges.
 *      .
 *      It also has one big disadvantage:
 *      - The tag reader APP has no control over what can be read out.
 *      .
 *      The application seamlessly switches from this 'automatic mode' to conventional command - response exchanges when
 *      the tag reader starts writing in the NFC memory.
 *
 *  - The application firmware code driving this behavior can be found at:
 *      - @c Execute (@c maintlogger.c) @n
 *          The notification that an NFC write by the tag reader is completed is stored in the variable
 *          @c sTargetWritten and checked in @c Execute. There the NDEFT2T module is used to retrieve
 *          the data using #NDEFT2T_GetMessage and #NDEFT2T_GetNextRecord, which is then fed to the <em>msg</em> module
 *          using #Msg_HandleCommand.
 *          The <em>msg</em> module guarantees the creation of one response, which is subsequently given to
 *          @c ResponseCb (@c msghandler.c), where the response will be wrapped in a MIME record in an NDEF message.
 *      - #AppMsgInit @n
 *          When this function is called two commands with message IDs #MSG_ID_GETVERSION and #MSG_ID_GETNFCUID will be
 *          generated. Both commands are fed to the <em>msg</em> module and their responses stored in SRAM in
 *          @c ResponseCb.
 *      - @c GenerateNextAutomaticCommand (@c maintlogger.c)
 *          - After calling #AppMsgInit, #NDEFT2T_EnableAutomaticMode is called, followed by the generation of the first
 *              'automatic command' in @c GenerateNextAutomaticCommand. The first command generated this way has
 *              #APP_MSG_ID_GETCONFIG as message ID.
 *              When the <em>Get Config</em> response arrives in @c ResponseCb, the two stored responses with message
 *              IDs #MSG_ID_GETVERSION and #MSG_ID_GETNFCUID are joined together in the same NDEF message - but in
 *              different MIME records - and the output of #Text_GetStatus, #Text_GetFailures and #Text_GetTemperature
 *              is used to add one or more text record in that same message.@n
 *              The collection of all these records forms the initial NDEF message. This is the data that, at
 *              startup, must be copied fast enough by the ARM to the NFC memory to ensure a correct detection by the
 *              corresponding Android APP. It is sufficient to reset the 'automatic command' sequence - using
 *              @c sResetAutomaticCommandGeneration - and to subsequently call @c GenerateNextAutomaticCommand to
 *              re-generate this initial NDEF message.
 *          - Each time the tag reader reads out the NFC memory, the NDEFT2T module notifies the application. That
 *              notification is stored in @c sMessageRead, and checked in @c Execute which then
 *              triggers a new call to @c GenerateNextAutomaticCommand. This sequence is continually executed until one
 *              NFC write action of the tag reader is detected by the NFC controller.
 *          .
 *      .
 *  .
 *
 */
