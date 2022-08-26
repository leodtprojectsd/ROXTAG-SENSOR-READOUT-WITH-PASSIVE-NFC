/*
 * Copyright 2016-2019 NXP
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
#include "common.h"

#ifdef SLAVE

/**
 * I2C Bit Rate to be used.
 * See @ref NSS_CLOCK_RESTRICTIONS "SW Clock Restrictions" for allowed bit rates.
 */
#define I2C_BITRATE 100000
#if I2C_BITRATE > 400000
    #error "I2C_BITRATE is too high."
#endif

/**
 * System Clock rate to be used.
 * See @ref NSS_CLOCK_RESTRICTIONS "SW Clock Restrictions" for allowed system clock frequencies.
 */
#define SYSTEMCLOCK 1000000
#if (SYSTEMCLOCK <= 500000) || ((SYSTEMCLOCK < 8000000) && (I2C_BITRATE > 100000))
    #error "SYSTEMCLOCK is too low; I2C_BITRATE cannot be reached for I2C slave functionality."
#endif

static uint8_t sRxBuffer[I2C_MASTER_TX_SIZE];
static uint8_t sText[1205] =
        "Our NTAG SmartSensor portfolio offers single-chip solutions that combine NFC connectivity with autonomous "
        "sensing, data processing, and logging. These devices can be easily combined with other companion chips, such "
        "as radios or sensor solutions. NTAG SmartSensor devices are ideal for Internet of Things (IoT) products. \n"
        "Applications \n"
        "NTAG SmartSensor can be used in applications like personal health, patient adherence, logistics, packaging, "
        "agriculture, industry 4.0, or animal tagging. \n"
        "Internet of Things \n"
        "NTAG SmartSensor is one of the smallest edge computing devices – collected data is uploaded into the cloud "
        "via the NFC interface. As most smartphones have an NFC interface, data collection and reading are intuitive "
        "with dedicated apps on the phone. \n"
        "Temperature Calibration \n"
        "NHS3100 and NHS3152 are individually temperature-calibrated in production and ISO/IEC 17025 calibration "
        "certificates with NIST traceability are available on nxp.com. \n"
        "Temperature Characteristics \n"
        "Absolute accuracy of 0.3°C in the range of 0 to 40°C \n"
        "Absolute accuracy of 0.5°C in the range of -40 to 0°C and 40 to 85°C \n"
        "Temperature recalibration is not required for applications that need either accuracy profile.";
static I2C_XFER_T sXfer;
static void Slave_EventHandler(I2C_ID_T id, I2C_EVENT_T event);
static void Slave_Init(void);

/* ------------------------------------------------------------------------- */

/** Called under interrupt. */
void I2C0_IRQHandler(void)
{
    if (Chip_I2C_IsMasterActive(I2C0)) {
        Chip_I2C_MasterStateHandler(I2C0);
    }
    else {
        Chip_I2C_SlaveStateHandler(I2C0);
    }
}

/**
 * Called under interrupt.
 */
static void Slave_EventHandler(I2C_ID_T id, I2C_EVENT_T event)
{
    ASSERT(id == I2C0);
    (void)id; /* suppress [-Wunused-parameter]: only one value is possible, which is unused below. */
    if (event == I2C_EVENT_SLAVE_RX) {
        LED_On(LED_ALL);
        /* A byte has been received. The bytes sent by the master indicate in our application the offset from which
         * to start reading.
         */
        int offset = (int)((sRxBuffer[0] + ((unsigned int)sRxBuffer[1] << 8)) % sizeof(sText));
        sXfer.txBuff = &sText[offset];
        sXfer.txSz = (offset + I2C_SLAVE_TX_SIZE < (int)sizeof(sText)) ? I2C_SLAVE_TX_SIZE : ((int)sizeof(sText) - offset + 1);
    }

    if (event == I2C_EVENT_SLAVE_TX) {
        /* A byte has been transmitted. All data to transmit was already prepared; nothing left to do here. */
    }

    if (event == I2C_EVENT_DONE) {
        sXfer.rxBuff = sRxBuffer;
        sXfer.rxSz = sizeof(sRxBuffer) + 1;
        /* sXfer.txBuff corrected later */
        /* sXfer.txSz = corrected later */
        LED_Off(LED_ALL);
    }
}

static void Slave_Init(void)
{
    if (SYSTEMCLOCK == NSS_SFRO_FREQUENCY) {
        Chip_Flash_SetHighPowerMode(true);
    }
    Chip_Clock_System_SetClockFreq(SYSTEMCLOCK);
    Board_Init();

    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_4, IOCON_FUNC_1 | IOCON_I2CMODE_STDFAST);
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_5, IOCON_FUNC_1 | IOCON_I2CMODE_STDFAST);

    Chip_SysCon_Peripheral_DeassertReset(SYSCON_PERIPHERAL_RESET_I2C0);

    Chip_I2C_Init(I2C0);
    Chip_I2C_SetClockRate(I2C0, I2C_BITRATE);

    /* Finish initialization for slave I2C communication. */
    sXfer.slaveAddr = I2C_SLAVE_ADDRESS << 1;
    sXfer.rxBuff = sRxBuffer;
    sXfer.rxSz = sizeof(sRxBuffer) + 1;
    sXfer.txBuff = NULL; /* Corrected later */
    sXfer.txSz = 0; /* Corrected later */

    Chip_I2C_SlaveSetup(I2C0, I2C_SLAVE_2, &sXfer, Slave_EventHandler, 0);

    NVIC_EnableIRQ(I2C0_IRQn);

    /* No extra initialization required for slave-build functionality. */
}

/* ------------------------------------------------------------------------- */

int main(void)
{
    Slave_Init();

    for(;;) {
        ; /* All handling is done under interrupt in Slave_EventHandler */
    }
}

#endif
