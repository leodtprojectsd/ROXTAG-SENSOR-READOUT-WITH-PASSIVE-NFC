/*
 * Copyright 2015-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include <string.h>
#include "board.h"
#include "ndeft2t/ndeft2t.h"
#include "compress/compress.h"
#include "storage/storage.h"
#include "event/event.h"
#include "event_tag.h"
#include "temperature.h"
#include "msghandler.h"
#include "msghandler_protocol.h"
#include "memory.h"
#include "timer.h"
#include "validate.h"

/* ------------------------------------------------------------------------- */

/**
 * Use the define @c APP_MAINTAIN_SWD_CONNECTION to start debugging without the risk of losing the debug connection due
 * to power-off or deep power down.
 * When this is enabled, measurements are still taken according to the configuration, and communication is always
 * possible, both via SWD and via NFC.
 *
 * Enable this in a debug build configuration only, and during development and debugging only!
 *
 * @c APP_MAINTAIN_SWD_CONNECTION can also be defined in your Project settings.
 * Under LPCXPresso: Project > Properties > C/C++ Build > Settings > Tool Settings > MCU C Compiler > Symbols
 * @note When this define is enabled, #APP_NO_MEASUREMENT_IN_NFC_FIELD is ignored.
 */
#undef APP_MAINTAIN_SWD_CONNECTION
#ifdef APP_MAINTAIN_SWD_CONNECTION
    #define USEPLACEHOLDER false
#elif defined(APP_NO_MEASUREMENT_IN_NFC_FIELD)
    #define USEPLACEHOLDER true
#else
    #define USEPLACEHOLDER false
#endif

/**
 * @def APP_NO_MEASUREMENT_IN_NFC_FIELD
 * We can do a proper temperature measurement while in an NFC field, but the resulting value is not
 * representative as the NFC field will heat up the IC quickly and significantly.
 * Instead, a placeholder value can be used, but this requires careful interpretation on the host side.
 * The define @c APP_NO_MEASUREMENT_IN_NFC_FIELD allows you to switch between two different approaches:
 * - Always make a proper measurement regardless of the presence of the NFC field
 *  This is the default behavior.
 * - Do _not_ make a measurement when an NFC field is present but instead store a placeholder value.
 *  Define @c APP_NO_MEASUREMENT_IN_NFC_FIELD in your Project settings.
 *  Under LPCXPresso: Project > Properties > C/C++ Build > Settings > Tool Settings > MCU C Compiler > Symbols
 *  Take care to add this to all build configurations you want them added to.
 */

/**
 * @def BOARD_ENABLE_WAKEUP
 * Define @c BOARD_ENABLE_WAKEUP in your board header file.
 * This define serves two purposes:
 * - We can accept a configuration, but defer logging until a start trigger is given. That trigger is either receiving a
 *  separate start command - #APP_MSG_ID_START - or detecting the WAKEUP pin is pulled low. The latter can be enabled
 *  or disabled.
 *  The define @c BOARD_ENABLE_WAKEUP allows you to switch between two different approaches:
 *  - When undefined: Do _not_ allow to start via the WAKEUP-pin.
 *  - When defined: Monitor the WAKEUP pin when a configuration as been given but the start is delayed indefinitely -
 *      using the parameter #APP_MSG_DELAY_START_INDEFINITELY. When the WAKEUP pin detects a low, a first measurement
 *      is made.
 *  .
 * - The temperature logger Demo PCBs have a button attached to the WAKEUP pin. This define enables a check -
 *  performed at startup only - on the WAKEUP pin. When the pin is actively pulled low - by pushing the button and
 *  holding it down - execution is then paused until the pin is high again. The implementation is done in #ResetISR,
 *  and provides a break-in possibility for a debugger connected via SWD to halt the ARM core before the SWD
 *  functionality is (possibly) turned off in firmware.
 * .
 */

#if !(MEMORY_FIRSTUNUSEDEEPROMOFFSET < EVENT_EEPROM_FIRST_ROW*64)
    #error The SW memory map as defined through the default diversity settings and those overridden in app_sel.h is wrong. Fix it.
#endif

#if !(EVENT_EEPROM_LAST_ROW < STORAGE_EEPROM_FIRST_ROW)
    #error The SW memory map as defined through the default diversity settings and those overridden in app_sel.h is wrong. Fix it.
#endif
#if !(ALON_WORD_SIZE <= STORAGE_FIRST_ALON_REGISTER)
    #error The SW memory map as defined through the default diversity settings and those overridden in app_sel.h is wrong. Fix it.
#endif

/* ------------------------------------------------------------------------- */

/**
 * Just assign a reasonable number. It must accommodate for all the overhead that comes with the complete ndef
 * message, plus it must be a multiple of 4.
 */
#define MAX_COMMAND_MESSAGE_SIZE 0x44

/**
 * In seconds.
 * Allow the - assumed - corresponding host to issue a first command in the given time window.
 */
#define FIRST_HOST_TIMEOUT 1

/**
 * In seconds.
 * We're assuming the host will continually exchange commands and responses. If after some timeout no command
 * has been received, we abort communication and shut down. When a field is still present, we will automatically
 * wake up again from Deep Power Down and refresh the NFC shared memory with a new initial message.
 * This way we prevent a possible hang-up when both sides are waiting indefinitely for an NDEF message to be
 * written by the other side.
 * No need to set the timeout too strict: set it reasonably long enough to never hamper any execution flow,
 * while still being short enough to re-enable communication from a user perspective.
 */
#define HOST_TIMEOUT 20

/**
 * In seconds.
 * From what was observed, the power off/power on/(re-)select sequence takes place in the order of a few 100ms
 * at most. Waiting a full second seems more than enough to also take small changes in physical placement into
 * account.
 */
#define LAST_HOST_TIMEOUT 1

/**
 * In seconds.
 * No need to set the watchdog timeout too strict: set it long enough to never hamper any execution flow,
 * while still being short enough to re-enable communication from a user perspective.
 * Just ensure it is higher than #HOST_TIMEOUT.
 */
#define WATCHDOG_TIMEOUT (HOST_TIMEOUT + 5)

/* ------------------------------------------------------------------------- */

void App_FieldStatusCb(bool isPresent);
void App_MsgAvailableCb(void);
void App_MsgReadCb(void);
int App_CompressCb(int eepromByteOffset, int bitCount, void * pOut);
int App_DecompressCb(const uint8_t * pData, int bitCount, void * pOut);

static void Init(void);
static void InitApp(void);
#ifndef APP_MAINTAIN_SWD_CONNECTION
static void DeInit(const bool waitBeforeDisconnect);
#endif
static void DoPeriodicMeasurements(const bool usePlaceholder);
static bool Execute(PMU_DPD_WAKEUPREASON_T wakeupReason);
static void GenerateNextAutomaticCommand(void);

int main(void); /**< Application's main entry point. Declared here since it is referenced in ResetISR. */

/* ------------------------------------------------------------------------- */

/**
 * - Set to @c true in #App_MsgAvailableCb when a new NDEF message is available.
 * - Set to @c false in the loop #Execute when the new NDEF message is read out.
 * .
 */
static volatile bool sMessageAvailable = false;

/**
 * - Set to @c true in #App_MsgReadCb when a current NDEF message in NCF shared memory is read by the NFC tag reader.
 * - Set to @c false in the loop #Execute when the new NDEF message is written.
 */
static volatile bool sMessageRead = false;

/** Set to @c true to reset the command sequence as generated by #GenerateNextAutomaticCommand */
static bool sResetAutomaticCommandGeneration = true;

/* ------------------------------------------------------------------------- */

/**
 * Handler for (ARM) Reset Interrupt.
 * Implements and overrides the WEAK function in the startup module.
 */
void ResetISR(void)
{
    /* Increasing the system clock as soon as possible to reduce the startup time:
     * the NFC tag reader times out
     * - on Android after ~40ms (although some phones will wait up to 100 ms)
     * - on iOS after ~16ms
     *
     * Setting the clock to 2MHz is the maximum: when
     * - running without a battery
     * - at 4MHz
     * - when the field is provided by some phones (e.g. S5)
     * a voltage drop to below 1.2V was observed - effectively resetting the chip.
     */
    Chip_Clock_System_SetClockFreq(2 * 1000 * 1000);

    Board_Init();
#ifdef BOARD_ENABLE_WAKEUP
    while (!(NSS_GPIO->DATA[1 << 0])) {
        ; /* Wait for as long as the WAKEUP pin (0) is low. */
    }
#endif

#ifndef DEBUG
    Chip_WWDT_Start(WATCHDOG_TIMEOUT);
    /* The watchdog will be fed - by calling Chip_WWDT_Feed - in Execute in a while loop when the NFC
     * field is present. A reset will then occur when in either the main context or in an interrupt too many seconds are
     * spent, i.e. when it is stuck somewhere.
     */
#endif

    Startup_VarInit();
    main();
}

#ifdef BOARD_ENABLE_WAKEUP
/**
 * Handler for PIO0_0 / WAKEUP pin.
 * Implements and overrides the WEAK function in the startup module.
 */
void PIO0_0_IRQHandler(void)
{
    Chip_SysCon_StartLogic_ClearStatus(SYSCON_STARTSOURCE_PIO0_0);
    if (Memory_IsReadyToStart()) {
        Memory_AddToState(APP_MSG_EVENT_STARTING, true);
        /* Configure the time to make a first measurement: start the RTC-down counter.
         * A first measurement will then be made in the main context, not under this interrupt.
         * Cheat a bit: assume 'within 1 second' is as good as 'immediately'.
         */
        Timer_StartMeasurementTimeout(1);
    }
}
#endif

/**
 * Called under interrupt.
 * @see NDEFT2T_FIELD_STATUS_CB
 * @see pNdeft2t_FieldStatus_Cb_t
 */
void App_FieldStatusCb(bool isPresent)
{
    if (isPresent) {
        Timer_StartHostTimeout(HOST_TIMEOUT);
    }
    else {
        /* A PCD (Proximity Coupled Device, i.e. the NFC tag reader) can do very strange things, a.o. power off the
         * field and immediately power on the field again, or 10+ times select the same device as part of its illogic
         * procedure to select a PICC (Proximity Integrated Circuit Card, i.e. the NFC tag) and start communicating
         * with it. (I'm primarily pointing to NFC-enabled Android phones now.)
         * Instead of deciding to Power-off or go to Deep Power Down immediately, it is more robust to additionally
         * check if during a small interval no NFC field is started again.
         * The loop in Execute() may thus not look at the NFC interrupt status, but at the result of
         * the timer interrupt.
         */
        Timer_StartHostTimeout(LAST_HOST_TIMEOUT);
    }
}

/**
 * Called under interrupt.
 * @see NDEFT2T_MSG_AVAILABLE_CB
 * @see pNdeft2t_MsgAvailable_Cb_t
 */
void App_MsgAvailableCb(void)
{
    sMessageAvailable = true;
    Timer_StartHostTimeout(HOST_TIMEOUT);
}

/**
 * Called under interrupt.
 * @see NDEFT2T_MSG_READ_CB
 * @see pNdeft2t_MsgRead_Cb_t
 */
void App_MsgReadCb(void)
{
    sMessageRead = true;
    Timer_StartHostTimeout(HOST_TIMEOUT);
}

/* ------------------------------------------------------------------------- */

/**
 * Connects the compress module with the storage module. Provides compression of data.
 * @see STORAGE_COMPRESS_CB
 * @see pStorage_CompressCb_t
 */
int App_CompressCb(int eepromByteOffset, int bitCount, void * pOut)
{
    ASSERT(bitCount == STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BITS);
    (void)bitCount; /* suppress [-Wunused-parameter]: its value is known to be STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BITS. */
    uint8_t data[STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BYTES];
    Chip_EEPROM_Read(NSS_EEPROM, eepromByteOffset, data, STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BYTES);
    int length = Compress_Encode(data, STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BYTES, pOut, STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BYTES);
    return length * 8;
}

/**
 * Connects the compress module with the storage module. Provides decompression of data.
 * @see STORAGE_DECOMPRESS_CB
 * @see pStorage_DecompressCb_t
 */
int App_DecompressCb(const uint8_t * pData, int bitCount, void * pOut)
{
    int length = Compress_Decode(pData, STORAGE_IDIVUP(bitCount, 8), pOut, STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BYTES);
    return (length == STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BYTES) ? STORAGE_UNCOMPRESSED_BLOCK_SIZE_IN_BITS : 0;
}

/* ------------------------------------------------------------------------- */

/**
 * - Initializes drivers and modules. Initialization is expected to be done once per complete active lifetime.
 * - Calls #InitApp.
 * .
 */
static void Init(void)
{
    (void)Temperature_Measure(TSEN_7BITS, false);
    NDEFT2T_Init();
    Timer_Init();
    InitApp();
}

/**
 * Initializes and configures APP modules. Initialization is expected to be done multiple times per complete active
 * lifetime (always going through the cycle @c #DeInitAp - @c InitApp) when #APP_MAINTAIN_SWD_CONNECTION is defined.
 */
static void InitApp(void)
{
    bool accepted = Memory_Init();
    AppMsgInit(accepted);

    sResetAutomaticCommandGeneration = true;
    /* Trigger the generation of a multi-record message as initial response. Do this unconditionally:
     * - it's a tiny bit faster
     * - it will allow tag readers who tap later, while the IC is still awake, to start the communication properly.
     */
    GenerateNextAutomaticCommand();

#ifdef BOARD_ENABLE_WAKEUP
    if (Memory_IsReadyToStart()) {
        NVIC_EnableIRQ(PIO0_0_IRQn); /* PIO0_0_IRQHandler is called when this interrupt fires. */
        Chip_SysCon_StartLogic_SetEnabledMask(SYSCON_STARTSOURCE_PIO0_0);
    }
#endif
}

/**
 * De-initializes APP modules. De-Initialization is expected to be done multiple times per complete active
 * lifetime (always going through the cycle @c DeInitApp - #InitApp) when #APP_MAINTAIN_SWD_CONNECTION is defined.
 */
static void DeInitApp(void)
{
    Memory_DeInit();
}

#ifndef APP_MAINTAIN_SWD_CONNECTION
/**
 * - Calls #DeInitApp
 * - Cleanly closes everything down, decides which low power state to go to - Deep Power Down or Power-off, and enters
 *  that state using the correctly determined parameters.
 * .
 * @note Wake-up conditions other than a reset pulse or the presence of an NFC field - both of which cannot be disabled
 *  nor require configuration - must have been set beforehand.
 * @param waitBeforeDisconnect Present to aid the SW developer. If @c true, it will wait - not sleep - for a couple of
 *  seconds before disconnecting the battery. The argument has no effect if Deep Power Down mode is selected.
 *  The extra time window allows for easier breaking in using SWD, allowing time to halt the core and flash new
 *  firmware. However, it will @c not touch any PIO, or ensure that SWD functionality is offered.
 *  The default value should be @c false, i.e. go to Power-off state without delay: typical user behavior is to bring
 *  the IC in and out the NFC field quickly, before stabilizing on a correct position. Having a time penalty of several
 *  seconds - during which the host SW may already have made several decisions about the use and state of the IC -
 *  diminishes the user experience.
 * @warning This function may return.
 */
static void DeInit(const bool waitBeforeDisconnect)
{
    bool isReadyToStart = Memory_IsReadyToStart();
    bool isMonitoring = Memory_IsMonitoring();
    bool isFull = Memory_IsFull();

    /* Before going to DPD mode, we need to determine whether switching must be enabled. We enable switching
     * when the battery is low, to ensure we can use the NFC interface as power supply in case the battery dies while
     * in DPD mode.
     * It is assumed that when the BOD detector indicates a battery voltage of 1.8 V or higher, the time spent in DPD
     * mode is not enough to drain the battery to a value below 1.72 V.
     */
    bool bod;
    if (isReadyToStart || isMonitoring || isFull) {
        Chip_PMU_SetBODEnabled(true);
        bod = ((Chip_PMU_GetStatus() & PMU_STATUS_BROWNOUT) != 0);
        Chip_PMU_SetBODEnabled(false);
        if (bod) {
            Memory_AddToState(APP_MSG_EVENT_BOD, true);
        }
    }
    bod = Memory_BodOccurred();

    DeInitApp();
    Chip_EEPROM_Flush(NSS_EEPROM, true);
    NDEFT2T_DeInit();

    /* Protection against Deep power down weirdness: when it fails and enters Deep sleep instead, make sure it leaves
     * it again after a short time. If going to Deep power down mode succeeds, or when going to Power-off, the timer
     * dies too and will not be able to wake the IC up from these states. Thus, starting it has no negative side
     * effects.
     * See the documentation in the PMU driver.
     */
    Timer_StartHostTimeout(1);

    if (isReadyToStart) {
#ifdef BOARD_ENABLE_WAKEUP
        Chip_PMU_SetWakeupPinEnabled(true);
#endif
        Chip_PMU_PowerMode_EnterDeepPowerDown(bod);
        /* This function should not return. If it does, it means the WAKEUP pin has been toggled again. */
    }
    else if (isMonitoring || isFull) {
        /* When full, we still want to keep track of time. The timer is not stopped in DoPeriodicMeasurements when
         * a measurement cannot be stored, so we will still wake up periodically (making a futile attempts at storing a
         * new measurement), and can then decide on the correct argument value for
         * Chip_PMU_PowerMode_EnterDeepPowerDown.
         */
        Chip_PMU_PowerMode_EnterDeepPowerDown(bod);
        /* This function should not return. If it does, it means the WAKEUP pin has been toggled again. */
    }
    else {
        if (waitBeforeDisconnect) {
            Chip_Clock_System_BusyWait_ms(2000); /* Give some extra time to intervene via SWD. */
        }
        Chip_PMU_Switch_OpenVDDBat();
        /* Normally, this function never returns. However, when providing power via SWD or any other PIO this will not
         * work - current is flowing through the bondpad ring via the SWD pin, still powering a small part of the
         * VDD_ALON domain.
         * This situation is not covered by HW design: we can't rely on anything being functional or even harmless.
         * Just ensure nothing happens, and wait until all power is gone.
         */
         for(;;);
    }
}
#endif

/**
 * To be used in 'automatic' mode, where the NFC tag reader does not issue any command, but instead continually reads
 * the NFC shared memory, expecting the firmware to timely update the contents. That is done by waiting for a 'message
 * read' callback issued by NDEFT2T - #App_MsgReadCb, and then create a next command ourselves and feed it to
 * #AppMsgHandleCommand.
 * @note The generation of new commands can be reset by setting #sResetAutomaticCommandGeneration to @c true.
 */
static void GenerateNextAutomaticCommand(void)
{
    /* In order:
     * 0, 1: APP_MSG_ID_GETCONFIG
     * 2: APP_MSG_ID_GETEVENTS all except about periodic events
     * 3: APP_MSG_ID_GETEVENTS only about periodic events
     * 4: APP_MSG_ID_GETMEASUREMENTS <n> times
     * 5: MSG_ID_GETRESPONSE
     */
    static int sIndex = 0; /* To keep track which command needs to be generated */
    static int sOffset = 0; /* To keep track of the offset when generating APP_MSG_ID_GETMEASUREMENTS a next time. */

    const uint32_t periodicEvents = APP_MSG_EVENT_TEMPERATURE_TOO_HIGH | APP_MSG_EVENT_TEMPERATURE_TOO_LOW;
    const APP_MSG_CMD_GETEVENTS_T getEventsCommand2 = {.index = 0,
                                                       .eventMask = APP_MSG_EVENT_ALL & ~periodicEvents,
                                                       .info = EVENT_INFO_INDEX | EVENT_INFO_TIMESTAMP
                                                               | EVENT_INFO_ENUM};
    const APP_MSG_CMD_GETEVENTS_T getEventsCommand3 = {.index = 0,
                                                       .eventMask = periodicEvents,
                                                       .info = EVENT_INFO_INDEX | EVENT_INFO_TIMESTAMP
                                                               | EVENT_INFO_ENUM | EVENT_INFO_DATA};
    APP_MSG_CMD_GETMEASUREMENTS_T getMeasurementsCommand = {.offset = 0};

    if (sResetAutomaticCommandGeneration) {
        sResetAutomaticCommandGeneration = false;
        NDEFT2T_EnableAutomaticMode();
        sIndex = 0;
    }

    uint8_t cmd; /* MSG_ID_T or APP_MSG_ID_T */
    uint8_t * pParam = NULL;
    int paramLength = 0;

    switch (sIndex) {
        case 0:
            /* See comment in App_FieldStatusCb. On Android, there are multiple read attempts before the low-level NFC
             * selection process is finished and the NFC connection is kept stable on the tag reader side.
             * For the ARM, this means it better keeps the first message stable until it can be sure the message has
             * been read and handled by the final and intended application.
             * After trial and error, it seems that allowing/enforcing one extra redundant read is a safe value.
             */
        case 1: cmd = APP_MSG_ID_GETCONFIG; sIndex++; break;
        case 5:
        default: cmd = MSG_ID_GETRESPONSE; sIndex++; break;

        case 2:
            cmd = APP_MSG_ID_GETEVENTS;
            pParam = (uint8_t *)&getEventsCommand2;
            paramLength = sizeof(APP_MSG_CMD_GETEVENTS_T);
            sIndex++;
            break;

        case 3:
            cmd = APP_MSG_ID_GETEVENTS;
            pParam = (uint8_t *)&getEventsCommand3;
            paramLength = sizeof(APP_MSG_CMD_GETEVENTS_T);
            sIndex++;
            break;

        case 4:
            if (sOffset <= 0) {
                sOffset = Storage_GetCount();
            }
            if (sOffset <= 0) {
                cmd = MSG_ID_GETRESPONSE;
                sIndex++;
            }
            else {
                cmd = APP_MSG_ID_GETMEASUREMENTS; /* Send back the most recent not-yet-sent measurements */
                if (sOffset >= APP_MSG_MAX_TEMPERATURE_VALUES_IN_RESPONSE) {
                    sOffset -= APP_MSG_MAX_TEMPERATURE_VALUES_IN_RESPONSE;
                }
                else {
                    sOffset = 0;
                    sIndex++;
                }
                getMeasurementsCommand.offset = (uint16_t)sOffset;
                pParam = (uint8_t *)&getMeasurementsCommand;
                paramLength = sizeof(APP_MSG_CMD_GETMEASUREMENTS_T);
            }
            break;
    }

    uint8_t data[2 + paramLength];
    data[0] = cmd;
    data[1] = 0;
    if (pParam) {
        memcpy(data + 2, pParam, (size_t)paramLength);
    }
    AppMsgHandleCommand(2 + paramLength, data);
}

/**
 * Perform all actions required when one measurement is due.
 * @param usePlaceholder Indicates whether a placeholder must be stored (@c true) or a proper temperature measurement
 *  must be performed (@c false).
 */
static void DoPeriodicMeasurements(const bool usePlaceholder)
{
    const MEMORY_CONFIG_T * config = Memory_GetConfig();

    /* Entering this function means we are logging or are about to make a first measurement. In the latter case the
     * state is still APP_MSG_EVENT_STARTING.
     */
    if (!(config->status & APP_MSG_EVENT_LOGGING)) {
        Memory_AddToState(APP_MSG_EVENT_LOGGING, true);
    }

    /* First prepare the next wake-up moment. */
    uint32_t startTime = 0;
    bool found = Event_GetFirstByTag(EVENT_TAG_LOGGING, NULL, NULL, NULL, &startTime);
    ASSERT(startTime != 0);
    ASSERT(found);
    (void)found;
    int elapsed = Chip_RTC_Time_GetValue(NSS_RTC) - (int)startTime;
    /* runningTime: enter if running indefinitely.
     * startTime: enter if logging state is not yet reached.
     * elapsed: enter if time is not expired
     */
    if ((config->cmd.runningTime == 0) || ((elapsed >= 0) && (elapsed < (int)config->cmd.runningTime))) {
        /* The RTC consists of two timers. The down counter is used to leave Deep power down mode, but stops when it
         * reaches 0. The up counter is never stopped. The time spent during booting, measuring temperature and
         * reaching this point is not tracked by the down counter, but can be detected using the up counter.
         * When we see an accumulated untracked time of more than 1 second (and thus we enter the if block below),
         * we reduce the sleep time with 1 second. On average, we're then still on track.
         *
         * The check (deviation > 1) is performed as (elapsed - sampleCount * interval > 1)
         */
        int interval = (int)config->cmd.interval;
        if ((Storage_GetCount() * interval) + 1 < elapsed) {
            if (interval > 1) {
                interval--;
            }
        }
        Timer_StartMeasurementTimeout(interval);
    }
    else {
        if (elapsed < 0) {
            /* This block should never be entered: there is no logical scenario in which the program counter ends up in
             * here. The only reason why elapsed can be negative, is when the RTC timer got reset, which indicates a
             * BOD or a reset pulse. In Memory_Init, this would then be detected, and the state adjusted accordingly:
             * a call to this function would then never be triggered.
             * Still, we're here now. And there is no sense in making further measurements. Just do nothing. Safest
             * seems just to mark this end state and error condition - again, as it should be superfluous.
             */
            Memory_AddToState(APP_MSG_EVENT_STOPPED | APP_MSG_EVENT_BOD, true);
        }
        else {
            /* Enough time spent logging and monitoring. Do not schedule a new measurement. */
            Memory_AddToState(APP_MSG_EVENT_STOPPED | APP_MSG_EVENT_EXPIRED, true);
        }
        Timer_StopMeasurementTimeout();
    }

    /* Only now make the measurement. */
    Temperature_Reset();
    if (!usePlaceholder) {
        Temperature_Measure(TSEN_10BITS, false);
    }
    int data = Temperature_Get();
    if (Storage_Write((STORAGE_TYPE *)&data, 1) != 1) {
        /* Either compression failed; either storage is full. */
        Memory_AddToState(APP_MSG_EVENT_STOPPED | APP_MSG_EVENT_FULL, true);
        /* There are two possibilities here:
         * - either we just stop, save the battery and go to power-off. No need to stop the timer, since power-off is
         *  imminent.
         * - Or we continue to keep track of time, in which case it is most prudent to still wake up periodically so we
         *  can decide correctly on the value of the enableSwitching parameter of Chip_PMU_PowerMode_EnterDeepPowerDown.
         * Either way, no need to call Timer_StopMeasurementTimeout here.
         */
    }
    Validate_Temperature((STORAGE_TYPE)data);
}

/**
 * Implements a while loop, waiting for an NDEF message in the NFC shared memory. the Ndeft2t module is used for this.
 * When a message is detected, it is parsed as a 'command' in the msg module and a response is written in the NFC shared
 * memory.
 * This function returns when either
 * - no message is received in a reasonable time limit
 * - at least one message has been received (and answered to), and later the NFC field is removed.
 * .
 * @param wakeupReason : indicates the cause of the wakeup, and determines the tasks to be carried out.
 * @return @c true when at least one message has been read or received (valid or not); @c false otherwise.
 * @note When the RTC down counter reaches zero, #DoPeriodicMeasurements is called.
 */
static bool Execute(PMU_DPD_WAKEUPREASON_T wakeupReason)
{
    __attribute__ ((section(".noinit"))) __attribute__((aligned (4)))
    static uint8_t sData[MAX_COMMAND_MESSAGE_SIZE];

    __attribute__ ((section(".noinit"))) __attribute__((aligned (4)))
    static uint8_t sNdefInstance[NDEFT2T_INSTANCE_SIZE];

    switch (wakeupReason) {
        case PMU_DPD_WAKEUPREASON_RTC:
            DoPeriodicMeasurements(false);
            break;

        case PMU_DPD_WAKEUPREASON_WAKEUPPIN:
            if (Memory_IsReadyToStart()) {
                Memory_AddToState(APP_MSG_EVENT_STARTING, true);
                DoPeriodicMeasurements(false);
            }
            break;

        case PMU_DPD_WAKEUPREASON_NONE:
        case PMU_DPD_WAKEUPREASON_NFCPOWER:
        default:
            /* Nothing extra to do before entering the execution loop. */
            break;
    }

#ifndef DEBUG
    Chip_WWDT_Feed();
#endif
    /* Wait for a command. Send responses based on these commands. */
    bool messageRxTx = false;
    do {
        /* Do not call Chip_PMU_PowerMode_EnterSleep here: an interrupt can have occurred between the check above
         * and the call below, resulting in an everlasting while loop.
         */
        if (sMessageAvailable) {
            sMessageAvailable = false;
            messageRxTx = true;
            if (NDEFT2T_GetMessage(sNdefInstance, sData, sizeof(sData))) {
                const uint8_t * data;
                int length;
                NDEFT2T_PARSE_RECORD_INFO_T recordInfo;
                while (NDEFT2T_GetNextRecord(sNdefInstance, &recordInfo)) {
                    if (recordInfo.type == NDEFT2T_RECORD_TYPE_MIME) {
                        data = NDEFT2T_GetRecordPayload(sNdefInstance, &length);
                        AppMsgHandleCommand(length, data);
                    }
                }
            }
        }

        if (sMessageRead) {
            sMessageRead = false;
            messageRxTx = true;
            NDEFT2T_ResetNfcMemory();
            GenerateNextAutomaticCommand();
        }

        if (Timer_CheckMeasurementTimeout()) {
            DoPeriodicMeasurements(USEPLACEHOLDER);
        }
#ifndef DEBUG
        if ((Chip_NFC_GetStatus(NSS_NFC) & NFC_STATUS_SEL) != 0) {
            /* Only feed when NFC is still detected. This to avoid a hang where the while loop, checking on
             * Timer_CheckHostTimeout is never ending.
             */
            Chip_WWDT_Feed();
        }
#endif
    }
    while (!Timer_CheckHostTimeout());

    return messageRxTx;
}

/* -------------------------------------------------------------------------------- */

int main(void)
{
#ifdef APP_MAINTAIN_SWD_CONNECTION
    /* Avoid the use the Deep Power Down and Power-off low-power states and maintain a debugging connection over
     * SWD. Current consumption does not have focus here.
     */
    Init();
    for (;;) {
        Timer_StartHostTimeout(HOST_TIMEOUT);
        Execute(PMU_DPD_WAKEUPREASON_NONE);
        DeInitApp();
        InitApp();
    }
#else
    /* Normal operation. Use the Deep Power Down and Power-off low-power states and preserve battery as much as
     * possible.
     */
    Init();
    PMU_DPD_WAKEUPREASON_T wakeupReason = Chip_PMU_PowerMode_GetDPDWakeupReason();
    for (;;) {
        int timeout = 0; /* Try to spend the least possible time in active mode. */
        if ((wakeupReason == PMU_DPD_WAKEUPREASON_NFCPOWER) || (wakeupReason == PMU_DPD_WAKEUPREASON_NONE)) {
            /* Safest is to just try to communicate for a longer duration. */
            timeout = FIRST_HOST_TIMEOUT;
        }
        Timer_StartHostTimeout(timeout);

        /* waitBeforeDisconnect may only end up being true when two conditions are met:
         * - wakeupReason equals NONE or NFCPOWER
         * - no messages are read or written by the tag reader
         * When waitBeforeDisconnect is true, extra time is spent in active mode, allowing a debugger to attach to the
         * core.
         */
        bool waitBeforeDisconnect = (wakeupReason == PMU_DPD_WAKEUPREASON_NONE)
                || (wakeupReason == PMU_DPD_WAKEUPREASON_NFCPOWER);
        if (Execute(wakeupReason)) {
            waitBeforeDisconnect = false;
        }
        DeInit(waitBeforeDisconnect);
        /* Code execution is resumed. The same state as after a reset must be as closely met as possible. */
        Board_Init();
        Init();
        wakeupReason = PMU_DPD_WAKEUPREASON_WAKEUPPIN;
    }
#endif
    return 0;
}
