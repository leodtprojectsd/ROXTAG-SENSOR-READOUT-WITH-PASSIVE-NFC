/*
 * Copyright 2017-2018,2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __BOARD_H_
#define __BOARD_H_

#include "chip.h"

/** @defgroup LABEL_BOARD_NSS label: board support for a single component design.
 * @ingroup BOARDS_NSS
 * The label board is a minimalistic 'board' for the production of labels containing the NHS3100 IC as a single-chip
 * solution for temperature logging and monitoring.
 *
 * The label has no connections to external sensors or LEDs.
 *
 * The initialization function only performs initialization of the PIOs, minimizing current consumption. It does
 * not change the system clock configuration.
 *
 * @par Board support concept:
 *  Board support comes in the form of a statically-linked library which includes the bare minimum for an application
 *  to start the HW properly (IO pin configuration and/or clock configuration if required)
 *  as well as SW support for the features provided by the board.
 *  There is a single function that is part of the board support API: the board initialization function (#Board_Init).
 *  The board initialization has two distinct purposes. In one hand, to ensure that IO pin configuration and state are
 *  set to a harmless state for the respective board. On the other hand that the features provided by the board
 *  (LEDs, external memories, etc) are properly initialized in SW and ready to be used directly by the application.
 *  Whenever board support is needed for a different board (different pin layout, reduced/extended feature set, etc), a
 *  separate board library must be created, matching the requirements of the new board. As long as the initialization
 *  function prototype (#Board_Init) and the design is kept consistent between board APIs, applications may switch
 *  boards more easily during development or even be designed to run on different boards.
 *
 *  @par Diversity
 *  There are no modules included, and thus no diversity to tweak on a board level.
 *
 * @{
 */

/**
 * A unique define for the Development PCB.
 * Applications or modules can use this define to enable/disable code at compile time based on the board being used.
 */
#define BOARD_LABEL

/**
 * Depending on the peripherals on the board, power consumption is minimized by adding pull-ups or -downs on the digital
 * pins. These are added at startup in #Board_Init and should have these pulls when the pins are not in use.
 * @note possible values are #IOCON_RMODE_PULLDOWN, #IOCON_RMODE_PULLUP or #IOCON_RMODE_INACT (no pull).
 * @{
 */
#define BOARD_PIO0_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO1_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO2_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO3_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO4_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO5_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO6_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO7_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO8_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO9_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO10_PULL IOCON_RMODE_PULLDOWN
#define BOARD_PIO11_PULL IOCON_RMODE_PULLDOWN
/** @} */

/** If defined, a push button or similar is attached to PIO0 and can wake up the IC when in Deep Power Down. */
#undef BOARD_ENABLE_WAKEUP
#if defined(BOARD_ENABLE_WAKEUP) && (BOARD_PIO0_PULL != IOCON_RMODE_PULLUP)
    #error Wake up functionality cannot work without a pull-up on PIO0.
#endif

/**
 * Sets up and initializes all required blocks and functions related to the board hardware.
 * @note Initialization will add pulls according to #BOARD_PIO0_PULL, #BOARD_PIO1_PULL and so on, to the default IO pin
 *  configuration.
 * @note Initialization does not change the system clock configuration.
 * @pre This function may be only called once, may be only called after a hard reset, and must be called as soon as
 *  possible.
 * @post These chip level drivers are initialized and ready for use:
 *  - @ref IOCON_NSS "IOCON"
 *  - @ref GPIO_NSS "GPIO"
 *  - @ref RTC_NSS "RTC"
 *  - @ref EEPROM_NSS "EEPROM"
 *  - @ref NFC_NSS "NFC"
 *  .
 * @note This function may be called @b before Startup_VarInit has been called.
 */
void Board_Init(void);

#endif /** @} */
