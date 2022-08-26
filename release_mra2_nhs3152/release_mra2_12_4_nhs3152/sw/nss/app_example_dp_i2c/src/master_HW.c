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

#include <string.h>
#include "board.h"
#include "ndeft2t/ndeft2t.h"
#include "i2cbbm/i2cbbm.h"
#include "common.h"

#ifdef MASTER_HW

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
#if (SYSTEMCLOCK <= 500000) || ((SYSTEMCLOCK < 4000000) && (I2C_BITRATE > 100000))
    #error "SYSTEMCLOCK is too low; I2C_BITRATE cannot be reached for I2C master functionality."
#endif

static void Master_Init(void);
static bool Master_CommunicateWithSlave(const uint8_t * txData, int txSize, uint8_t * rxData, int rxSize);
static void Master_CreateNdef(const uint8_t * text, int size);

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

static void Master_Init(void)
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

    /* Finish initialization for master I2C communication. */
    Chip_I2C_SetMasterEventHandler(I2C0, Chip_I2C_EventHandler);
    NVIC_EnableIRQ(I2C0_IRQn);

    /* Extra initialization required for master-build functionality:
     * - prepare NDEF message creation
     * - Use pin 3 for i2c pull-up - assuming R3 and R4 are stuffed.
     */
    NDEFT2T_Init();
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_3, IOCON_FUNC_0);
    Chip_GPIO_SetPinDIROutput(NSS_GPIO, 0, 3);
    Chip_GPIO_SetPinOutHigh(NSS_GPIO, 0, 3);
}

static bool Master_CommunicateWithSlave(const uint8_t * txData, int txSize, uint8_t * rxData, int rxSize)
{
    I2C_XFER_T xfer = {.slaveAddr = I2C_SLAVE_ADDRESS,
                       .txBuff = txData,
                       .txSz = txSize,
                       .rxBuff = rxData,
                       .rxSz = rxSize};
    I2C_STATUS_T status = Chip_I2C_MasterTransfer(I2C0, &xfer);
    return status == I2C_STATUS_DONE;
}

static void Master_CreateNdef(const uint8_t * text, int size)
{
    uint8_t instance[NDEFT2T_INSTANCE_SIZE] __attribute__((aligned (4)));
    uint8_t message[NFC_SHARED_MEM_BYTE_SIZE] __attribute__((aligned (4)));
    NDEFT2T_CREATE_RECORD_INFO_T createRecordInfo = {.shortRecord = true, .pString = (uint8_t *)"en"};
    NDEFT2T_CreateMessage(instance, message, NFC_SHARED_MEM_BYTE_SIZE, true);
    NDEFT2T_CreateTextRecord(instance, &createRecordInfo);
    NDEFT2T_WriteRecordPayload(instance, text, size);
    NDEFT2T_CommitRecord(instance);
    NDEFT2T_CommitMessage(instance);
}

/* ------------------------------------------------------------------------- */

int main(void)
{
    Master_Init();
    uint16_t offset = 0;
    uint8_t text[I2C_SLAVE_TX_SIZE + 1]; /* + NUL char */
    for (;;) {
        LED_On(LED_ALL);
        if (Master_CommunicateWithSlave((uint8_t *)&offset, I2C_MASTER_TX_SIZE, text, I2C_SLAVE_TX_SIZE)) {
            /* The slave may have responded with less data than I2C_SLAVE_TX_SIZE bytes.
             * These will end up as 0xFF at the end of the buffer.
             */
            text[sizeof(text) - 1] = 0;
            uint8_t * p = (uint8_t *)strchr((char *)text, 0xFF);
            Master_CreateNdef(text, (p == NULL) ? I2C_SLAVE_TX_SIZE : (p - text));
        }
        LED_Off(LED_ALL);
        Chip_Clock_System_BusyWait_ms(2000);
        offset = (uint16_t)(offset + 66);
    }
}

#endif
