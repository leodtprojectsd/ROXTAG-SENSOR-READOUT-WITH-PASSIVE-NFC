/*
 * Copyright 2015-2016 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

#ifndef __APP_SEL_H_
#define __APP_SEL_H_


// For each module xxx this project reuses from 'mods' project, it must define the macro
// XXX_CFG in this file. Furthermore, the macros must either be 0 (and module xxx
// will use its default configuration mods/xxx/xxx_dft.h) or the macro must
// be 1 (and module xxx will use <proj>/mods/xxx_cfg.h). In the latter case, 
// this project must add xxx_cfg.h to its mods directory.


//#define XXX_CFG 0
//#define YYY_CFG 0


#endif /* __APP_SEL_H_ */
