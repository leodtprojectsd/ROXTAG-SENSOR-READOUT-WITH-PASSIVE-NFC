/** @defgroup APP_NSS_SSP ssp: SSP Driver Example
 * @ingroup APPS_NSS
 * Example application that demonstrates SSP master and slave functionality to send and receive data in both 
 * polling and interrupt modes using the @ref SSP_NSS "SSP Driver".
 *
 * @par Introduction
 *  This SSP application serves as an example application to demonstrate how to make use of the @ref SSP_NSS "SSP Driver".
 *  This application demonstrates the configuration of the SSP as Master and slave for sending and receiving data in both 
 *  polling and interrupt modes. It also demonstrates the loopback mode.@n
 *  The different functionalities demonstrated by the application are available as separate build configurations, namely:
 *   -# Debug - Debug build for Master rx/tx in loopback mode
 *   -# Debug_Master_Interrupt - Debug build for Master rx/tx in interrupt mode
 *   -# Debug_Slave_Interrupt - Debug build for Slave rx/tx in interrupt mode
 *   -# Debug_Master_Polling - Debug build for Master rx/tx in polling mode
 *   -# Debug_Slave_Polling - Debug build for Slave rx/tx in polling mode
 *   .
 *
 * @par How to setup
 *  User can use two NHS demo boards for the example application. The SSP pins - @b PIO0_2, @b PIO0_6, @b PIO0_8 and 
 *  @b PIO0_9 - of the two boards are to be connected back to back. @n
 *  To evaluate this example, user can flash one of the boards with the master build and the other board with the
 *  slave build. Only one board and build is required for loopback mode.
 * 
 * @par User Interaction
 *  Start the slave first and then the master. An external SSP monitor can be used to monitor the data transfer. In
 *  addition the RED LED on the board will be ON in case an error is encountered during the operation.
 *
 * @par Use Case Description
 *  The SSP application transfers 256 bytes (which are the numbers 0 to 255) from one end to the other. Receive buffer on 
 *  each side is initialized with 0xAA before the transfer starts, which will then get overwritten by the actual received 
 *  data.
 *
 * @par Loopback Mode
 *  Demonstrates the loopback mode. For this mode, POLLING_MODE has to be set to 0, SSP_MODE_TEST to 1 and LOOPBACK_TEST to 1
 *
 * @par Master send/receive in Interrupt Mode
 *  Demonstrates the sending and receiving of data as Master in Interrupt mode. For this mode, POLLING_MODE has to be set to 0, 
 *  SSP_MODE_TEST to 1 and LOOPBACK_TEST to 0
 *
 * @par Master send/receive in Polling Mode
 *  Demonstrates the sending and receiving of data as Master in Polling mode. For this mode, POLLING_MODE has to be set to 1, 
 *  SSP_MODE_TEST to 1 and LOOPBACK_TEST to 0
 *
 * @par Slave send/receive in Interrupt Mode
 *  Demonstrates the sending and receiving of data as Slave in Interrupt mode. For this mode, POLLING_MODE has to be set to 0, 
 *  SSP_MODE_TEST to 0 and LOOPBACK_TEST to 0
 *
 * @par Slave send/receive in Polling Mode
 *  Demonstrates the sending and receiving of data as Slave in Polling mode. For this mode, POLLING_MODE has to be set to 1, 
 *  SSP_MODE_TEST to 0 and LOOPBACK_TEST to 0
 *
 */
