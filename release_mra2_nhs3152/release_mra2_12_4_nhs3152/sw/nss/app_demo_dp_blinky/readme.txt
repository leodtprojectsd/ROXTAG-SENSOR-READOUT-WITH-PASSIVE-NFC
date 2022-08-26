/** @defgroup APP_NSS_BLINKY blinky: "Hello World" Blinky Demo
 * @ingroup APPS_NSS
 * Demo application that perpetually keeps a LED blinking.
 *
 * @par Introduction
 *  The blinky application is one of the most simple applications.
 *  It toggles a GPIO pin with a frequency of 2Hz, which has a LED connected to it.
 *  A 250ms busy wait is introduced between each toggle, which results in a period of 500ms (2Hz).
 *
 * @par LED
 *  The LED's characteristics are defined in the board the blinky application links to, i.e. lib_board_dp. 
 *
 * @par Additional feature
 *  The main() function has one optional feature.
 *  It configures the PIO0_1 pin  as CLKOUT, outputting the ARM clock frequency.
 *  By hooking a scope to that pin, the ARM clock can be measured.
 */
