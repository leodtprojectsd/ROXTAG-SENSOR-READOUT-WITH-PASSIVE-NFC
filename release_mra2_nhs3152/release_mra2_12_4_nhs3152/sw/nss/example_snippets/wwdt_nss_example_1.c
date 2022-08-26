/*
 * Copyright 2016-2017,2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#include "board.h"

void wwdt_nss_example_1(void)
{
//! [wwdt_nss_example_1]
    Chip_GPIO_SetPinDIROutput(NSS_GPIO, 0, 1);
    Chip_GPIO_SetPinDIROutput(NSS_GPIO, 0, 2);
    Chip_GPIO_SetPinOutLow(NSS_GPIO, 0, 1);
    Chip_GPIO_SetPinOutLow(NSS_GPIO, 0, 2);
    Chip_WWDT_Start(3);

    Chip_Clock_System_BusyWait_ms(1111); /* Less than 3 seconds */
    Chip_GPIO_SetPinOutHigh(NSS_GPIO, 0, 1); /* This call will be executed */
    Chip_WWDT_Feed();   

    Chip_Clock_System_BusyWait_ms(2222); /* Less than 3 seconds */
    Chip_GPIO_SetPinOutLow(NSS_GPIO, 0, 1); /* This call will be executed */
    Chip_WWDT_Feed();

    Chip_Clock_System_BusyWait_ms(3333); /* Longer than 3 seconds */
    Chip_GPIO_SetPinOutHigh(NSS_GPIO, 0, 2); /* This call will never be executed */
//! [wwdt_nss_example_1]
}
