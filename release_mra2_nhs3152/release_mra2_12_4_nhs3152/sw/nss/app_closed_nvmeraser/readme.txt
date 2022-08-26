/**
 * @defgroup APP_NSS_NVMERASER nvmerase: NVM Erase program (closed)
 * @ingroup APPS_NSS
 * This closed application is a small tool that will erase all writable EEPROM pages (see #EEPROM_NR_OF_RW_ROWS) and
 * all writable FLASH pages (see #FLASH_NR_OF_RW_SECTORS) that are not occupied by this very program. Use this to
 * ensure you can start again from a clean slate.
 *
 * @par How to operate
 *  Simply flash the binary and reset the IC. The EEPROM will be erased first, followed by the minimal number of erase
 *  procedures to erase the FLASH pages.
 *  When that is done, the IC will go to power-off mode.
 *
 * @par ARM application overview
 *  Below the code flow of the application:
 *  - Wait some seconds.
 *  - Overwrite all #EEPROM_NR_OF_RW_ROWS rows with zeroes.
 *  - Wait some seconds.
 *  - Determine the first page not occupied by the closed application: @em f.
 *  - Erase all pages from that point @em f up to the start of a full sector: @em s.
 *  - Erase all sectors from @em s up #FLASH_NR_OF_RW_SECTORS - 1.
 *  - Wait some seconds.
 *  - Disconnect the battery.
 *  .
 *  In total the LED(s) will be lit three times, with short gaps in between.
 */
