/*
 * Copyright 2016-2018 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __TIMER_H_
#define __TIMER_H_

/** @addtogroup APP_DEMO_TLOGGER_TIMER Use-case specific timers
 * @ingroup APP_DEMO_TLOGGER
 * The timer functions provide a high-level abstraction API to access the different timers in an NHS31xx for specific
 * uses.
 *
 * - The 16-bit HW timer is used to help regulate the connection with the tag reader; since intermediate disconnections
 *  on HW NFC level do not indicate the end of the communication scheme: Android phones may connect and disconnect
 *  repeatedly before deciding on a stable connection; and iOS phones will always disconnect immediately after reading
 *  one NDEF message. @n
 *  See #Timer_StartHostTimeout, #Timer_StopHostTimeout, #Timer_CheckHostTimeout.
 * - The 32-bit HW timer is used to help in random number generation. When started, it simply run, allowing the
 *  application code to fetch it running counter and use that a random number @b provided the event deciding on the
 *  sampling cannot be predicted. @n
 *  See #Timer_StartFreeRunning, #Timer_StopFreeRunning, #Timer_GetFreeRunning.
 * - The down counter of the RTC HW block is used to know when a measurement is due.
 *  See #Timer_StartMeasurementTimeout, #Timer_StopMeasurementTimeout, #Timer_CheckMeasurementTimeout.
 * .
 *
 * @{
 */

#include <stdint.h>
#include <stdbool.h>

/**
 * Initialize the timers so that the other function calls become available.
 * @pre This must be the first call to this block of code.
 */
void Timer_Init(void);

/* -------------------------------------------------------------------------------- */

/**
 * Starts or restarts a timer. The timer will keep on running in Sleep and Deep Sleep modes.
 * @note The 16-bit timer is used. It will run as slow as possible, i.e the resolution will be low, assuming accuracy
 *  is not a concern here.
 * @param seconds The timeout interval. Use #Timer_CheckHostTimeout to check whether the interval has expired.
 *  Use #Timer_StopHostTimeout to check the status.
 * @pre @c seconds must be a strict positive number
 */
void Timer_StartHostTimeout(int seconds);

/**
 * Stops the 16-bit timer.
 * @post A call to #Timer_CheckHostTimeout will now return @c false.
 */
void Timer_StopHostTimeout(void);

/**
 * Check if time as set by #Timer_StartHostTimeout has elapsed.
 * @return @c True when the timer was started and the interrupt was fired. @c false otherwise.
 * @note When the timer has timed out, an explicit call to #Timer_StartHostTimeout is required to restart the
 *  16-bit timer.
 */
bool Timer_CheckHostTimeout(void);

/* -------------------------------------------------------------------------------- */

 /**
 * Starts or restarts a timer.
 * @note The RTC timer is used, which will continue running when going to the Deep Power Down mode, and wake up the IC
 *  when it expires.
 * @param seconds The timeout interval. Use #Timer_CheckMeasurementTimeout to check whether the interval has expired.
 * @pre @c seconds must be a strict positive number.
 * @post The internal status is reset: an immediate call to #Timer_CheckMeasurementTimeout will now return @c false.
 */
void Timer_StartMeasurementTimeout(int seconds);

/**
 * Stops the RTC timer.
 * @post An immediate or later call to #Timer_CheckMeasurementTimeout will now return @c false.
 */
void Timer_StopMeasurementTimeout(void);

/**
 * Check if time as set by #Timer_StartMeasurementTimeout has elapsed.
 * @return @c True when the timer was started and the interrupt was fired. @c false otherwise.
 * @note When the timer has timed out, an explicit call to #Timer_StartMeasurementTimeout is required to restart the
 *  RTC timer.
 */
bool Timer_CheckMeasurementTimeout(void);

/* -------------------------------------------------------------------------------- */

/**
 * Starts the 32-bit timer. The timer will keep on running in Sleep and Deep Sleep modes.
 * @note The 32-bit timer is used without setting any interrupts. It will run as fast as possible.
 */
void Timer_StartFreeRunning(void);

/**
 * Stops the 32-bit timer.
 * @post All subsequent call to #Timer_GetFreeRunning will now return the same value.
 */
void Timer_StopFreeRunning(void);

/**
 * Retrieve the current timer value.
 * @return A positive number. When the moment of calling this function is not deterministic, the outcome can be used as
 *  a source of entropy.
 */
uint32_t Timer_GetFreeRunning(void);

#endif /** @} */
