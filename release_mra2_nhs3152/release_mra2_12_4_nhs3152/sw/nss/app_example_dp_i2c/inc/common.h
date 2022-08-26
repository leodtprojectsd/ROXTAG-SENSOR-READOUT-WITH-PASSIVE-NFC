/*
 * Copyright 2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __COMMON_H_
#define __COMMON_H_

/**
 * @defgroup APP_NSS_I2C_COMMON common: defines shared between both master and slave.
 * @ingroup APP_NSS_I2C
 * Defines required to be known and in sync between both master and slave device.
 *
 * @{
 */

/**
 * The 7-bit (MSBit 0) slave address of the slave device.
 * - The master will attempt I2C communication to this address only;
 * - the slave will listen to this address only.
 * .
 */
#define I2C_SLAVE_ADDRESS 0x11

/**
 * In this example application, it is chosen to, in one I2C transaction, start with a short write, immediately followed
 * by a longer read. The size in bytes of the short write is captured in this define.
 */
#define I2C_MASTER_TX_SIZE 2

/**
 * In this example application, it is chosen to, in one I2C transaction, start with a short write, immediately followed
 * by a longer read. The size in bytes of the longer read is captured in this define.
 */
#define I2C_SLAVE_TX_SIZE 180

#endif /** @} */
