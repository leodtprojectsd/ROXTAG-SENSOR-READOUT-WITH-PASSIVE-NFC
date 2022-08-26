/*
 * Copyright 2015-2016,2018-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"

/*****************************************************************************
 * Private types/enumerations/variables
 ****************************************************************************/

#if POLLING_MODE
#define INTERRUPT_MODE      0
#else
#define INTERRUPT_MODE      1
#endif
#define BUFFER_SIZE                         (0x100)
#define SSP_DATA_BITS                       (SSP_BITS_8)
#define SSP_DATA_BIT_NUM(databits)          ((databits) + 1)
#define SSP_DATA_BYTES(databits)            (((databits) > SSP_BITS_8) ? 2 : 1)
#define SSP_LO_BYTE_MSK(databits)           ((SSP_DATA_BYTES(databits) > 1) ? \
        0xFF : (0xFF >> (8 - SSP_DATA_BIT_NUM(databits))))
#define SSP_HI_BYTE_MSK(databits)           ((SSP_DATA_BYTES(databits) > 1) ? \
        (0xFF >> (16 - SSP_DATA_BIT_NUM(databits))) : 0)
#define NSS_SSP           NSS_SSP0
#define SSP_IRQ           SSP0_IRQn
#define SSPIRQHANDLER     SSP0_IRQHandler

/* Tx buffer */
static uint8_t Tx_Buf[BUFFER_SIZE] __attribute__((aligned (4)));

/* Rx buffer */
static uint8_t Rx_Buf[BUFFER_SIZE] __attribute__((aligned (4)));

/** SSP config format */
typedef struct SSP_ConfigFormat {
    CHIP_SSP_BITS_T bits; /**< Format config: bits/frame */
    CHIP_SSP_CLOCK_MODE_T clockMode; /**< Format config: clock phase/polarity */
    CHIP_SSP_FRAME_FORMAT_T frameFormat; /**< Format config: SPI/TI/Microwire */
} SSP_ConfigFormat;

static SSP_ConfigFormat ssp_format;
static Chip_SSP_DATA_SETUP_T xf_setup;

/*****************************************************************************
 * Public types/enumerations/variables
 ****************************************************************************/

/* Configure pins in IOCON for SSP operation */
static void Init_SSP_PinMux(void)
{
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_8, IOCON_FUNC_1); /* MISO */
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_9, IOCON_FUNC_1); /* MOSI */
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_2, IOCON_FUNC_1); /* SSEL */
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_6, IOCON_FUNC_1); /* SCLK */
}

/*****************************************************************************
 * Private functions
 ****************************************************************************/

/* Initialize buffer */
static void Buffer_Init(void)
{
    uint16_t i;
    uint8_t ch = 0;

    for (i = 0; i < BUFFER_SIZE; i++) {
        Tx_Buf[i] = ch++;
        Rx_Buf[i] = 0xAA;
    }
}

/* Verify buffer after transfer */
static uint8_t Buffer_Verify(void)
{
    uint16_t i;
    uint8_t *src_addr = (uint8_t *)&Tx_Buf[0];
    uint8_t *dest_addr = (uint8_t *)&Rx_Buf[0];

    for (i = 0; i < BUFFER_SIZE; i++) {

        if (((*src_addr) & SSP_LO_BYTE_MSK(ssp_format.bits)) != ((*dest_addr) & SSP_LO_BYTE_MSK(ssp_format.bits))) {
            return 1;
        }
        src_addr++;
        dest_addr++;

        if (SSP_DATA_BYTES(ssp_format.bits) == 2) {
            if (((*src_addr) & SSP_HI_BYTE_MSK(ssp_format.bits)) != ((*dest_addr) & SSP_HI_BYTE_MSK(ssp_format.bits))) {
                return 1;
            }
            src_addr++;
            dest_addr++;
            i++;
        }
    }
    return 0;
}

/*****************************************************************************
 * Public functions
 ****************************************************************************/
#if INTERRUPT_MODE
/**
 * @brief   SSP interrupt handler sub-routine
 * @return  Nothing
 */
void SSPIRQHANDLER(void)
{
    if (SSP_DATA_BYTES(ssp_format.bits) == 1) {
        if ((Chip_SSP_Int_RWFrames8Bits(NSS_SSP, &xf_setup) == ERROR) ||
           ((xf_setup.rx_cnt >= xf_setup.length) && (xf_setup.tx_cnt >= xf_setup.length))) {
            Chip_SSP_Int_Disable(NSS_SSP);
        }
    }
    else {
        if ((Chip_SSP_Int_RWFrames16Bits(NSS_SSP, &xf_setup) == ERROR) ||
           ((xf_setup.rx_cnt >= xf_setup.length) && (xf_setup.tx_cnt >= xf_setup.length))) {
            Chip_SSP_Int_Disable(NSS_SSP);
        }
    }
}

#endif /*INTERRUPT_MODE*/

/**
 * Main routine for ssp example
 * @return Function should not exit.
 */
int main(void)
{
    /* Set system clock to 2MHz which is required for a bitrate of 100kbps used in this application. */
    Chip_Clock_System_SetClockFreq(NSS_SFRO_FREQUENCY/4);

    Board_Init();

    /* SSP initialization */
    Init_SSP_PinMux();
    Chip_SSP_Init(NSS_SSP);
    ssp_format.frameFormat = SSP_FRAME_FORMAT_SPI;
    ssp_format.bits = SSP_DATA_BITS;
    ssp_format.clockMode = SSP_CLOCK_MODE0;
    Chip_SSP_SetMaster(NSS_SSP, SSP_MODE_TEST);
    Chip_SSP_SetFormat(NSS_SSP, ssp_format.bits, ssp_format.frameFormat, ssp_format.clockMode);
    Chip_SSP_SetBitRate(NSS_SSP, 100000);

#if LOOPBACK_TEST
    Chip_SSP_EnableLoopBack(NSS_SSP);
#endif
    Buffer_Init();
    /* Setup SSP buffer */
    xf_setup.length = BUFFER_SIZE;
    xf_setup.tx_data = Tx_Buf;
    xf_setup.rx_data = Rx_Buf;
    xf_setup.rx_cnt = xf_setup.tx_cnt = 0;

#if POLLING_MODE
    Chip_SSP_Enable(NSS_SSP); /* Enable SSP after everything is setup */
    if (Chip_SSP_RWFrames_Blocking(NSS_SSP, &xf_setup) != BUFFER_SIZE) {
        LED_On(LED_RED); /* Error */
    }
#elif INTERRUPT_MODE
    Chip_SSP_ClearIntPending(NSS_SSP, SSP_INT_CLEAR_BITMASK);
    Chip_SSP_Enable(NSS_SSP); /* Enable SSP after everything is setup */
    Chip_SSP_Int_Enable(NSS_SSP); /* Enable SSP interrupt */
    NVIC_EnableIRQ(SSP_IRQ); /* Enable SSP interrupt in NVIC */
    while(!Chip_SSP_GetStatus(NSS_SSP, SSP_STAT_TFE)); /* Wait till Transmit FIFO is empty */
#endif /*INTERRUPT_MODE*/

    if (Buffer_Verify()) {
        LED_On(LED_RED); /* Error */
    }

#if LOOPBACK_TEST
    Chip_SSP_DisableLoopBack(NSS_SSP);
#endif

    Chip_SSP_Disable(NSS_SSP); /* Disable SSP */
    Chip_SSP_DeInit(NSS_SSP); /* DeInitialize SPI peripheral */

#if INTERRUPT_MODE
    Chip_SSP_Int_Disable(NSS_SSP); /* Disable SSP interrupt */
    NVIC_DisableIRQ(SSP_IRQ); /* Disable SSP interrupt in NVIC */
#endif /*INTERRUPT_MODE*/

    while (1) {
    }
    return 0;
}
