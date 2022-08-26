# Passive NFC Sensor Readout
This Repo contians all the information to make a Passive NFC based Sensor readout, based on measuring DC resitnace.  

This Repo  uses an NFC based RFID chip (NHS3152) to detect a resistance of a device under test (DUT). An RFID is a tag that harvest EM energy to power a microchip and communicate with a device (in our case, an Android Phone). The project was born to Measure the resistance of a radiation sensor (relating changes in R to radiation absorbed), however it generalized to any DUT that needs a DC resistance measurement. The system is completely passive, harvesting the EM radiation to i) power the chip, ii) probe the DUT iii)communicate with reader (eg. phone), allowing to work battery free. This is useful for enviromental sensing applications, and anything where a battery integration would be too bulky/impractival/expensive. 

<b>Project structure</b><br />
The project is divided in 3 parts: 
 - <b>NHS3152-App</b><br />
 App written in Java (programmer Alessandro Rossi), the app handles the phone-Chip NFC communication, captures the resistance data, and dose some basic plotting.  

 - <b>NHS3152-Firmware</b><br />
 The on-chip firmware, this code written in c allows the chip to measure the resistance of the DUT, save the data to memory, and communicate the data to the phone.  

 - <b> NHS3152-Hardware</b><br /> 
 This PCB design integrates the chip with an antenna (for energy harvesting and communication), and provides pads to connect to the DUT.  

```
├───NHS3152-App 

│   ├───app 

│   │   ├───build 

│   │   │   ├───outputs 

│   │   │   │   ├───apk 

│   │   │   │   │   └───debug 

│   │   └───src 

│   │       ├───androidTest 

│   │       │   └───java 

│   │       │       └───com 

│   │       │           └───example 

│   │       │               └───readernhs 

│   │       ├───main 

│   │       │   ├───java 

│   │       │   │   └───com 

│   │       │   │       └───example 

│   │       │   │           └───readernhs 

│   └───NHS3152APPdocs 

│       ├─── HowREADERNHSAPPworks.pdf  

│       ├─── ProjectReport.pdf  

│       ├─── READERNHS3152 APP Documentation.pdf  

│       └───tutorialVideo.mp4 

│        

├───NHS3152-Firmware 

│   ├─── NHS3152 Firmware CODE DOC.pdf  

│   ├─── NHS3152 IDE Getting started.pdf  

│    └─── README.md  

│       

│   ├───DC_R_Measure 

│   │   ├───mods 

│   │   └───src 

│   ├───lib_board_dp 

│   │   ├───inc 

│   │   ├───mods 

│   │   └───src 

│   ├───lib_chip_nss 

│   │   ├───inc 

│   │   ├───mods 

│   │   └───src 

│   └───mods 

│       ├───batimp 

│       ├───compress 

│       │   └───heatshrink 

│       ├───diag 

│       ├───event 

│       ├───i2cbbm 

│       ├───led 

│       ├───msg 

│       ├───ndeft2t 

│       ├───startup 

│       ├───storage 

│       ├───tmeas 

│       └───uarttx 

└───NHS3152-Hardware 

   ├─── Hardware Design PCB documentation.pdf  

   ├─── NHS3152 Demo PCB.zip 

    └───Version1-NXP-VALIDATION 

        ├───Assembly 

        ├───DrillFiles 

        ├───GerberFiles 

        └───ODBFiles 

            └───odb 
└───release_mra2_12_4_nhs3152  
```
