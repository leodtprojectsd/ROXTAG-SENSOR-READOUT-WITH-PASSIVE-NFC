/**
 * @defgroup APP_NSS_NFCPROGRAMLOADER nfcprogramloader: NFC one-time Program Loader (closed)
 * @ingroup APPS_NSS
 * This closed application acts as a second stage bootloader, placed at the highest writable sectors in flash
 * (see #FLASH_NR_OF_RW_SECTORS).
 * It gives the customer the possibility to program the IC via NFC (no wired connection needed).
 *
 * @par Introduction
 *  The NFC program loader facilitates wireless download of the final product application binary into the NHS31xx IC.
 *  This allows on-the-field programming of the required custom application when SWD is no longer accessible. 
 *
 * @note Once an application binary is flashed using the NFC program loader, the NFC program loader won't be available
 *  anymore.
 *  the IC will immediately boot into the flashed application and no other application image can be flashed via NFC.
 *
 * @par Get started (and how to operate)
 *  The NHS31xx IC initially (after leaving power-off state) writes an initial message to the NFC block, containing 
 *  version information.
 *  After that, the host can start the download by simply sending the first data packet.
 *  The very last packet has a different message ID and includes a file CRC.
 *  Detailed information about the protocol can be found in the protocol submodule
 *  (@ref APP_NSS_NFCPROGRAMLOADER_MSGHANDLER_PROTOCOL).
 *
 * @par ARM application overview
 *  The top level flow of the application is described in the following diagram:
 *  @dot
 *      digraph {
 *          node [shape = box, style=rounded]
 *          rankdir="LR"
 *
 *          subgraph a {
 *              rank=same
 *              "Start (Power On)" -> "App Initialization"
 *              "App Initialization" -> "Sleep" [ label = "Sleep until\npacket received" ]
 *              "TimeOut?" [shape = diamond]
 *              "Sleep" -> "TimeOut?" [label = "Woke up\n(Timeout/NFC)"]
 *              "Open PowerSwitch and hang"
 *               "TimeOut?" -> "Open PowerSwitch and hang" [label = "Yes"]
 *          }
 *          subgraph b {
 *              rankdir="LR"
 *              "Process package"
 *              "Download\nongoing?" [shape = diamond]
 *              "Process package" -> "Download\nongoing?" [shape = diamond]
 *          }
 *
 *          "TimeOut?" -> "Process package" [label = "No\n(Packet in)"]
 *
 *          "Download\nongoing?" -> "Open PowerSwitch and hang" [ label = "No"];
 *          "Download\nongoing?" -> "Sleep" [ label = "Yes"];
 *
 *      }
 *  @enddot
 *
 * @warning The nfcprogramloader is not designed to be used in NFC passive power mode. FLASH write operations will
 *  result in voltage drops, causing the IC to reset.
 */
