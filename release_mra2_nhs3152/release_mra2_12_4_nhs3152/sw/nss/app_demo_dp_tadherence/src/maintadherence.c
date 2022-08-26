/*
 * Copyright 2015-2018,2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"
#include "memorymanager.h"
#include "msghandler.h"
#include "therapy.h"

/**
 * The following diagram describes the states and their transitions:
 *
 *    Therapy was not active
 *    OR setPrestine command received     __________
 *            +------------------------->|          |              All pills are taken
 *            |                       +->|Power-Off |<----------+-----------------------+
 *            |                       |  |__________|           |                       |
 *            |                       |       |                 |Stopped                |
 *      ______|___                    |       |                 |                       |
 *     |          |  NFC field present|       |           ______|___________           _|___________
 *     | NFC comm.|<------------------+       |resetN    |                  |         |             |
 *     |__________|    Woken up by NFC|       +--------->|   Stop Therapy   |         | Sense Pills |<-+
 *            |                       |       |resetN    |__________________|         |_____________|  |
 *            |                       |       |                                         |              |
 *            |                       |       |                                         |Pills         |
 *            |                       |   ____|_____                                    |present       |
 *            |                       |  |          |                                   |              |
 *            |                       +--| DPD mode |<----------------------------------+              |
 *            |                          |__________|                                                  |
 *            |Therapy becomes active         |                                                        |
 *            |Therapy was active             |RTC Interrupt                                           |
 *            +-------------------------------+--------------------------------------------------------+
 */

/** Enum describing all possible actions to be performed after waking up from DPD or POWEROFF */
typedef enum ACTION_ID {

    /**
     * Action where communication over NFC is possible, give the other party some time to send us a command.
     * Next step:
     * - ACTION_UPDATETHERAPY if previous state was DPD
     * - Goto state DPD if start command received (therapy is started).
     * - Goto state POWEROFF If previous state was POWEROFF or If setPrestine command received (therapy is reseted).
     * .
     */
    ACTION_ID_NFCCOMMUNICATION,

    /**
     * Action where the actual sensing is taking place + update of the therapy state.
     * Next state:
     * - DPD if pills present
     * - POWEROFF if all pills removed (therapy ended)
     * .
     */
    ACTION_ID_UPDATETHERAPY,

    /**
     * Action: resetting the therapy to save the battery (ResetN).
     * - If therapy is ongoing -> reset therapy
     * - If therapy not yet started -> nothing, default state is already power off.
     * .
     *
     * Next state:
     * - POWEROFF if therapy was ended
     * .
     */
    ACTION_ID_RESETTHERAPY
} ACTION_ID_T;

static void Init(void);
static void DeInit(void);
static void StartWakeupTimer(int seconds);
static void StopWakeupTimer(void);
static void ResetTherapy(void);

/** Flag indicating whether after communication a the therapy needs an update. */
static volatile bool sTherapyUpdatePending;

int main(void)
{
    Init();

    /* Determine the previous state and the action to perform. */
    PMU_DPD_WAKEUPREASON_T wakeup = Chip_PMU_PowerMode_GetDPDWakeupReason();
    ACTION_ID_T action;
    if (PMU_DPD_WAKEUPREASON_NONE == wakeup) { /* Not woken up from DPD so woken up from Power-Off */
        /* PREVIOUS STATE: POWEROFF
         * Possible events:
         * -NFC field detected; action: #ACTION_NFCCOMMUNICATION
         * -ResetN; action: #ACTION_TOGGLETHERAPY
         * .
         * determine current action:
         */
        if (Chip_PMU_Int_GetRawStatus() & PMU_INT_NFCPOWER) {
            /* Woken up due to NFC field
             * After communication therapy update is not needed since chip woke up from power-off state
             * (No therapy ongoing) */
            action = ACTION_ID_NFCCOMMUNICATION;
        }
        else { /* Woke up due to an event other than NFC. Expect it to be ResetN. */
            action = ACTION_ID_RESETTHERAPY;
        }
    }
    else { /* Woken up from DPD */
        /* PREVIOUS STATE: DPD
         * -ResetN; action: #ACTION_TOGGLETHERAPY -> resetN forces previous state to #POWEROFF so handled above.
         * -NFC field detected; action: #ACTION_NFCCOMMUNICATION
         * -RTC interrupt; action: #ACTION_UPDATETHERAPY
         * .
         * determine current action:
         */
        if (PMU_DPD_WAKEUPREASON_NFCPOWER == wakeup) {
            action = ACTION_ID_NFCCOMMUNICATION;
            /* We do not know how long the chip will stay in NFC field.
             * Since in DPD a therapy is always ongoing, it is the easiest
             * to just update the therapy status before going to DPD again. */
            sTherapyUpdatePending = true;
        }
        else {
            action = ACTION_ID_UPDATETHERAPY;
        }
    }

    /* Perform determined action */
    switch (action) {
        case ACTION_ID_RESETTHERAPY:
            ResetTherapy();
            break;
        case ACTION_ID_NFCCOMMUNICATION:
        default:
            MsgHandler_NFCCommunication();
            if (!sTherapyUpdatePending) {
                break; /* Only break if no update is needed */
            }
            /* fallthrough */
        case ACTION_ID_UPDATETHERAPY:
            Therapy_Update();
            sTherapyUpdatePending = false;
            break;
    }

    DeInit(); /* Next state, POWEROFF or DPD, is decided on in #DeInit  */
    return 0; /* Should never come here since DeInit does not return. */
}

/** Init function, makes sure that all action independent parts are initialized. */
static void Init(void)
{
    Board_Init();
    Memory_Init();
    Therapy_Init();
    sTherapyUpdatePending = false;
}

/**
 * DeInit function, makes sure that all action independent parts are properly De-initialized.
 * This function is responsible for proper state transition (after action) to DPD or POWEROFF.
 */
static void DeInit(void)
{
    Therapy_DeInit();
    Memory_DeInit();
    if (THERAPY_STATE_ID_ONGOING == Therapy_GetState()) {
        StartWakeupTimer((int)Therapy_GetCheckPeriod()); /* Reprogram the next wake-up */

        /* Before going to DPD mode, we need to determine whether switching must be enabled. We enable switching when the
         * battery is low, to ensure we can use the RFID interface as power supply in case the battery dies while in DPD
         * mode. It is assumed that when the BOD detector indicates a battery voltage of 1.8 V or higher, the time spent in
         * DPD mode is not enough to drain the battery to a value below 1.72 V.
         */
        bool enableSwitching;
        Chip_PMU_SetBODEnabled(true);
        enableSwitching = ((Chip_PMU_GetStatus() & PMU_STATUS_BROWNOUT) != 0);
        Chip_PMU_SetBODEnabled(false);
        Chip_PMU_PowerMode_EnterDeepPowerDown(enableSwitching);
        ASSERT(false); /* May never come here, as the previous call does not return. */
    }
    else {
        StopWakeupTimer();
        Chip_PMU_Switch_OpenVDDBat();
        while (1) {
            ; /* Chip might be powered via NFC */
        }
    }
}

/**
 * Stops the therapy, this will cause a change in therapy status, resulting in going to Power-off mode (save battery).
 */
static void ResetTherapy(void)
{
    LED_On(LED_RED);
    Chip_Clock_System_BusyWait_ms(100);
    LED_Off(LED_RED);
    /* Make sure a debugger can connect. */
    Chip_Clock_System_BusyWait_ms(4000);
    if (THERAPY_STATE_ID_ONGOING == Therapy_GetState()) {
        /* Ongoing therapy so stop the therapy to save the battery. */
        Therapy_Reset();
    }
}

/**
 * Starts or restarts the RTC wake-up downcounter.
 * @param seconds The timeout interval. After this many seconds an interrupt will be fired (chip is woken up).
 */
static void StartWakeupTimer(int seconds)
{
    Chip_RTC_Wakeup_SetControl(NSS_RTC, RTC_WAKEUPCTRL_DISABLE);
    Chip_RTC_Int_SetEnabledMask(NSS_RTC, RTC_INT_WAKEUP);
    Chip_RTC_Int_ClearRawStatus(NSS_RTC, RTC_INT_ALL);
    Chip_RTC_Wakeup_SetControl(NSS_RTC, RTC_WAKEUPCTRL_ENABLE | RTC_WAKEUPCTRL_AUTO);
    Chip_RTC_Wakeup_SetReload(NSS_RTC, seconds);
}

/**
 * Stops the RTC wake-up downcounter, chip will not wake up from RTC anymore.
 */
static void StopWakeupTimer(void)
{
    Chip_RTC_Wakeup_SetControl(NSS_RTC, RTC_WAKEUPCTRL_DISABLE);
}
