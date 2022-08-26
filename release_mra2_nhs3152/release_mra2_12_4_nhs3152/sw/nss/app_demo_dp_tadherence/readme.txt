/** @defgroup APP_DEMO_TADHERENCE tadherence: Therapy Adherence Demo Application
 * @ingroup APPS_NSS
 * The Therapy Adherence Demo application will demonstrate the value proposition around therapy adherence use case.
 * This application together with a smart blister or a demo PCB and an Android application will serve as a starting
 * point for app developers to develop a full blown solution helping patients adhering better to their therapy. 
 *
 * @par Introduction
 *  This demo application is designed to run on an NHS31xx IC mounted on an Demo PCB or Demo foil. 
 *  Depending on the sensing principle the correct NHS family member should be chosen. 
 *  Please refer to @ref APP_DEMO_TADHERENCE_THERAPY_SENSE_SPECIFIC to understand the sensing principles available for your
 *  NHS IC.@n
 *  In the ARM application, regardless of the sensing principle, the present pills are divided into groups, these groups
 *  are defined by the drive pin of a certain subset of the pills.
 *
 * @par ARM application overview
 *  The aim of the ARM application is to (once the therapy is started), keep track of pill removals. Therefore the chip
 *  will be periodically woken up to update its therapy status (re-check the pill precedence). For every removal, an
 *  event is stored in non-volatile memory such that, later, a host device (smartphone) can read these via NFC.
 *  Most of the UI is done over NFC (NDEFT2T) using a complementary android application.@n
 *  The following user actions may be triggered via NFC (using a dedicated smartphone app.):
 *  - Start a new therapy
 *  - Configure the therapy
 *  - Stop an ongoing therapy
 *  - Reset the "smart-blister"
 *  - Read out the therapy adherence (how many/which pills were/weren't taken)
 *  .
 *  Besides this way of interaction (via dedicated smartphone app.), basic therapy status will be exposed via standard
 *  NDEF TXT records which can be read out and displayed by any NFC capable smartphone, thus, without the need of a dedicated app.
 *  In a reduced top level view, this application is formed by the main application file and several SW components which,
 *  in turn, rely on underlying infrastructure drivers and middleware modules (see respective component documentation):
 *      - @ref APP_DEMO_TADHERENCE_THERAPY
 *      - @ref APP_DEMO_TADHERENCE_MSGHANDLER
 *      - @ref APP_DEMO_TADHERENCE_MEMORY
 *      - @ref APP_DEMO_TADHERENCE_TXT
 *      - @ref APP_DEMO_TADHERENCE_THERAPY_SENSE
 *      .
 *
 *  The top level flow of the application is described in the following diagram:
 *  @dot
 *      digraph FlowDiagram {
 *          node [shape = box, style=rounded];
 *          Start[label = "Power Saving Mode \n DPD if Therapy ongoing \n Power Off if Therapy Stopped/not started", style=filled];
 *          Init[label = "App Initialization"];
 *          Update[label = "Update Therapy \n (check pill removals)"]
 *          Reset[label = "Reset therapy\n(not started)" ]
 *          NFC[label = "NFC\ncommunication" ]
 *          DeInit[label = "App De-Initialization"];
 *
 *          node [shape = diamond]; 
 *          WakeUp[label="Wake-up\nreason"];
 *          PostNFC[label = "Therapy\nOngoing?"];
 *
 *          Start -> Init [ label = "Wake Up" ];
 *          Init -> WakeUp;
 *          WakeUp -> Update [ label = "Periodic\nwake-up" ];
 *          WakeUp -> Reset [ label = "Reset pin\ntoggled" ];
 *          WakeUp -> NFC [ label = "NFC field\ndetected" ];
 *          NFC -> PostNFC;
 *          PostNFC -> Update [ label = "Yes" ];
 *          PostNFC -> DeInit [ label = "No" ];
 *          Update -> DeInit;
 *          Reset -> DeInit;
 *          DeInit -> Start;   
 *      }
 *  @enddot
 *
 *  The top level design of the application is clarified in the following block diagram:
 *  @dot
 *      digraph BlockDiagram {
 *          graph [rankdir = TB];
 *
 *          node[shape=record];
 *          nodesep=1.0
 *          
 *          Main[label="Main"];
 *          
 *          Msg[label="MsgHandler \n (NFC Communication)"];
 *          Ther[label="Therapy \n (managing the therapy (status))"];
 *          {rank=same;Msg Ther}
 *           
 *          Mem[label="Memory Manager"];
 *          Sense[label="Sense"];
 *          
 *          Prot[label="Protocol"];
 *          Text[label="Text"];
 *          SenseSp[label="Sense Specific \n (Principle dependent functionality)"];
 *          
 *          Main -> Msg [label=""];
 *          Main -> Ther [label=""];
 *          
 *          Msg -> Ther [label=""];
 *          Msg -> Mem [label=""];
 *          Msg -> Text [label=""];
 *          Msg -> Prot [label=""];
 *          
 *          Ther -> Sense [label=""];
 *          Ther -> Mem [label=""];
 *          
 *          Mem -> Text [label=""];
 *        
 *          Sense -> SenseSp [label=""];
 *      }
 *  @enddot
 *
 */
