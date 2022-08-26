/*
 * Copyright 2014-2016 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"

int main(void)
{
    Board_Init();

    /* Optional feature: send the ARM clock to PIO0_1 */
    Chip_IOCON_SetPinConfig(NSS_IOCON, IOCON_PIO0_1, IOCON_FUNC_1);
    Chip_Clock_Clkout_SetClockSource(CLOCK_CLKOUTSOURCE_SYSTEM);

    /* Blink with a period of 250ms+250ms, or 2Hz */
    while (1) {
        LED_Toggle(LED_RED);
        Chip_Clock_System_BusyWait_ms(250);
    }

    return 0;
}
