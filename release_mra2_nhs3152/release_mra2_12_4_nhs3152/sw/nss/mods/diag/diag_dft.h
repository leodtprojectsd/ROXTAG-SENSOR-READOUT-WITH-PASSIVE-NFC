/*
 * Copyright 2018 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

/**
 * @defgroup MODS_NSS_DIAG_DFT Diversity settings
 * @ingroup MODS_NSS_DIAG
 * These 'defines' capture the diversity settings of the module. The displayed values refer to the default settings.
 * To override the default settings, place the defines with their desired values in the chip chip_sel.h header file.
 * The compiler will pick up your defines before parsing this file.
 * @{
 */

#ifndef __DIAG_DFT_H_
#define __DIAG_DFT_H_

/**
 * Enable Diag module by default. It can be explicitly disabled without the need to remove the module by defining
 * ENABLE_DIAG_MODULE and assigning a @c 0 value.
 */
#ifndef ENABLE_DIAG_MODULE
    #define ENABLE_DIAG_MODULE 1
#endif

#endif /** @} */
