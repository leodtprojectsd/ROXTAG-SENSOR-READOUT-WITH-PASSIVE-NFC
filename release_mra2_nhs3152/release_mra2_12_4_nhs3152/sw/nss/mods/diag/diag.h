/*
 * Copyright 2018-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __DIAG_H_
#define __DIAG_H_

/**
 * @defgroup MODS_NSS_DIAG diag: Diagnostics module
 * @ingroup MODS_NSS
 *
 * This module collects basic information about the behavior of the IC and stores it in EEPROM. The purpose is to
 * fetch the stored data later for post-mortem analysis to help identifying an issue or to use it for performance
 * monitoring.
 *
 * Five statistics are gathered:
 *  - Number of cold boots
 *  - Number of wake-ups
 *  - Deep Power down time
 *  - Active time
 *  - Number of NFC taps
 *  .
 *
 * @par Number of cold boots
 *  The number of times a cold boot was detected is counted. A cold boot is generated when leaving the Power-off mode,
 *  when the RESETN pin has been toggled, or when the watchdog timer expires.
 *
 * @par Number of wake-ups
 *  The number of times a wake-up was detected is counted. A wake-up is generated when leaving the Deep power down mode.
 *  The reason of the wakeup - due to NFC, RTC timeout or WAKEUP pin toggle - is not tracked.
 *
 * @par Deep power down time
 *  The Deep power down time accumulates the time spent in Deep power down mode.
 *  - The time is reported in seconds.
 *  - The ARM RTC up counter is used to track the time.
 *  - Each time the Deep power down mode is entered, an accuracy loss of up to 1 second can occur.
 *  - Upon a power failure, the total last time spent while in Deep down mode is lost and untracked.
 *  .
 *
 * @par Active time
 *  The active time accumulates the time spent in Active, Sleep and Deep sleep modes. Time spent
 *  while in Deep Power Down or Power-off mode is excluded.
 *  - The time is reported in seconds.
 *  - The ARM RTC up counter is used to track the time.
 *  - The System up time will remain accurate even when the RTC timer is reset or reconfigured.
 *  - Upon a power failure, the total last time spent while Active (or Sleep or Deep sleep mode) is lost and untracked.
 *  .
 *  @note For a typical logging application, where the time spent in Active, Sleep and Deep sleep mode is in the order
 *      of 50 - 100 ms, the reported time here will only differ from 0 due to the time spent inside an NFC field.
 *
 * @par Number of NFC taps
 *  The number of times the tag was selected for NFC communication by a tag reader.
 *  - It is not tracked how long the tag was selected.
 *  - It is not tracked whether communication - NFC reads or writes - actually occurred.
 *  - Some tag readers enable and disable the NFC field multiple times as part of a single tap (I'm mainly looking at
 *      Android phones here). This is not an issue when waking up from Deep power down or Power-off, but may cause to
 *      false positives when an NFC field is applied while the IC was already in Active mode. Therefore, subsequent
 *      NFC field detections are only counted when the RTC up counter value has changed.
 *  .
 *
 * @par Diversity
 *  This module supports a single diversity #ENABLE_DIAG_MODULE.
 *  See @ref MODS_NSS_DIAG_DFT.
 *
 * @par Implementation
 *  To be able to track the above statistics, it relies on the @ref MODS_NSS_STARTUP startup module, the @ref PMU_NSS
 *  PMU driver and the @ref RTC_NSS RC driver to call its API when appropriate.
 *  The interrupt vector table in the startup module is also modified to intercept the appropriate
 *  interrupts, after which they are redirected to the application again using the normally expected weak interrupt
 *  handler.
 *
 *  The total implementation - here in this module, and also the accommodating changes made in the startup module,
 *  the PMU driver and the RTC driver - can be enabled or disabled without requiring any change in the board library or
 *  the application layer.
 *  @see ENABLE_DIAG_MODULE
 *
 *  Care is taken to flush all data to EEPROM in a timely way, without imposing a burden to the system.
 *
 *  @warning The diagnostics module assumes the behavior needs only be tracked when battery-powered. When powered
 *      passively, i.e. through the NFC harvested energy, the power is lost whenever the tag is removed from the
 *      tag reader. Statistics will not be updated in, as no intermediate writes to EEPROM of the gathered statistics
 *      are performed. This only happens the the application firmware explicitly decides to enter deep power down or
 *      power-off mode.
 *
 *  @note The diagnostics module assumes that all write accesses to the RTC and the PMU are done using the
 *      corresponding SW drivers. If these registers are accessed directly, the data gathered from the diagnostics
 *      module become useless.
 * @{
 */

#include "diag_dft.h"

/* ------------------------------------------------------------------------- */

#pragma pack(push, 1)
/** Statistics data, retrieved by #Diag_Get */
typedef struct DIAG_DATA_S {
    uint32_t coldBootCount; /**< Total number of cold boots. */
    uint32_t wakeUpCount; /**< Total number of wake-ups. */
    int32_t activeTime; /**< Accumulated Active, Sleep and Deep Sleep time in seconds. */
    int32_t deepPowerDownTime; /**< Accumulated Deep power down time in seconds. */
    uint32_t nfcTapCount; /**< Total number of NFC select states. */
} DIAG_DATA_T;
#pragma pack(pop)

/* ------------------------------------------------------------------------- */

#if ENABLE_DIAG_MODULE

/**
 * This function must be the first function to call in this module after leaving deep power down mode or power-off mode.
 * @note This function will not use any variable from the BSS or DATA section, so is safe to be called as early as any
 *  moment while #Startup_VarInit is executed. It is best called in that function.
 *
 * - Loads the statistics from EEPROM to SRAM
 * - Updates the statistics: cause of chip startup and deep power down duration if appropriate.
 * .
 * @post EEPROM is powered and initialized.
 * @see Startup_VarInit
 */
void Diag_Init(void);

/**
 * This function must be the last function to call in this module before entering deep power down mode or power-off
 * mode. If not called, the statistics gathered and changed during the last active period (Active, sleep and deep sleep
 * modes combined - are lost.
 * @note It is best called from #Chip_PMU_PowerMode_EnterDeepPowerDown and #Chip_PMU_Switch_OpenVDDBat.
 * @post EEPROM is powered and initialized.
 * @see Chip_PMU_PowerMode_EnterDeepPowerDown
 * @see Chip_PMU_Switch_OpenVDDBat
 */
void Diag_DeInit(void);

/**
 * Must be called by the RTC driver when the RTC up counter is changed on request of the application. This allows to
 * track the accumulated system up time.
 * @pre Must be called before the update is executed.
 * @param new : the new RTC up-counter value
 * @see Chip_RTC_Time_SetValue
 */
void Diag_TrackRtcUpdate(int new);

/** @return a pointer to the latest statistics in SRAM. */
const DIAG_DATA_T* Diag_Get(void);

/* ------------------------------------------------------------------------- */

/**
 * Interrupt handler for the NFC Read/Write Interrupt hook.
 * To be used instead of the application defined WEAK-defined interrupt #NFC_IRQHandler
 * @pre must be used by startup.c only
 * @see g_pfnVectors
 */
void Diag_NFC_IRQHandler(void);

#else
    #define Diag_Init(x)  /* void */
    #define Diag_DeInit(x)  /* void */
    #define Diag_TrackRtcUpdate(x)  /* void */
    #define Diag_Get(x) NULL
    #define Diag_NFC_IRQHandler NFC_IRQHandler
#endif

#endif /** @} */
