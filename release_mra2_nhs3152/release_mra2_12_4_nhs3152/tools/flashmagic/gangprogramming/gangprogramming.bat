@rem Copyright 2017 NXP
@rem This software is owned or controlled by NXP and may only be used strictly
@rem in accordance with the applicable license terms.  By expressly accepting
@rem such terms or by downloading, installing, activating and/or otherwise using
@rem the software, you are agreeing that you have read, and that you agree to
@rem comply with and are bound by, such license terms.  If you do not agree to
@rem be bound by the applicable license terms, then you may not retain, install,
@rem activate or otherwise use the software.

::  ______ _           _       __  __             _      
:: |  ____| |         | |     |  \/  |           (_)     
:: | |__  | | __ _ ___| |__   | \  / | __ _  __ _ _  ___ 
:: |  __| | |/ _` / __| '_ \  | |\/| |/ _` |/ _` | |/ __|
:: | |    | | (_| \__ \ | | | | |  | | (_| | (_| | | (__ 
:: |_|    |_|\__,_|___/_| |_| |_|  |_|\__,_|\__, |_|\___|
::   _____                     _____         __/ |                                 _       
::  / ____|                   |  __ \       |___/                                 (_)            
:: | |  __  __ _ _ __   __ _  | |__) | __ ___   __ _ _ __ __ _ _ __ ___  _ __ ___  _ _ __   __ _ 
:: | | |_ |/ _` | '_ \ / _` | |  ___/ '__/ _ \ / _` | '__/ _` | '_ ` _ \| '_ ` _ \| | '_ \ / _` |
:: | |__| | (_| | | | | (_| | | |   | | | (_) | (_| | | | (_| | | | | | | | | | | | | | | | (_| |
::  \_____|\__,_|_| |_|\__, | |_|   |_|  \___/ \__, |_|  \__,_|_| |_| |_|_| |_| |_|_|_| |_|\__, |
::                      __/ |                   __/ |                                       __/ |
::                     |___/                   |___/                                       |___/                           

::####################################################################
::This is the main batch file to execute Flash Magic Gang Programming
::It consist of 6 sections executed sequentially:
::	A. User Input
::	B. Text Files and Batch Files Creation
::	C. Batch Files Execution
::	D. Progress Logging
::	E. Result Messages
::	F. Text Files and Batch Files Deletion
::####################################################################

@echo off
setlocal ENABLEDELAYEDEXPANSION
@title=Flash Magic Gang Programming
goto main

::Functions
::##########


::Display Help
:help
echo Flash Magic Gang Programming
echo.
echo Interactive mode:
echo   1. Connect multiple LPC-Link2 debug boards to the PC.
echo   2. Wire each debug board with a NHS3100 ICs using the SWD lines.
echo   3. Run: GangProgramming.bat
echo   4. Input correct path of program to be flashed
echo   5. Press Enter
echo.
echo Non-interactive mode:
echo   1. Connect multiple LPC-Link2 debug boards to the PC.
echo   2. Wire each debug board with a NHS3100 ICs using the SWD lines.
echo   3. Run: GangProgramming.bat "path/to/firmware.hex"
echo.
echo General:
echo   "/?" or -h or --h    : Command line argument.
echo                          Shows this help and exits.
echo   path/to/firmware.hex : Command line argument.
echo                          The path to the firmware image to be flashed. 
echo                          When given, no user input will be requested.
echo   q                    : User input when in interactive mode
echo                          Quits the program
echo.
goto :eof


::Display progress title
:displayTitle
echo.
echo ###########################################################################
echo                                   %1
echo ###########################################################################
goto :eof


::Display date and time
:displayTime
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%" & set "Min=%dt:~10,2%" & set "Sec=%dt:~12,2%"

set date=!DD!-!MM!-!YYYY!
set time=!HH!:!Min!:!Sec!

set /a minute=!Min!
set /a second=!Sec!

echo Date:!date!   Time:!time!
goto :eof


:main
::######################################################################################################################

	:: A: << USER INPUT >>
	
::######################################################################################################################

	:: description: 1. Check if an argument is entered, and if it is:
	::						a. /? or -h or --help, then display Help
	::						b. a program path, then store the program path
	::				2. If no argument in entered:
	::						a. display user manual
	::						b. prompt user to enter a program path
	::				3. If "q" is entered as argument or user input, then quit the program.
	::				4. Verify if the program path exists:
	::						a. Yes: proceed to batch files creation
	::						b. No : jump back to input prompt
	
	::Initialize with argument flag to 0, indicating no argument is entered
	set /a arg=0
	
	::Check if user entered command line argument
	:checkArg
	if not "%1" == "" (
		::If entered /? or -h or --help, jump to :help
		if "%1" == "/?" goto :help
		if "%1" == "-h" goto :help
		if "%1" == "--help" goto :help
		::Store program path from argument
		set programPath=%1
		::Set argument flag to 1, indicating an argument is entered
		set /a arg=1
		goto :checkq	
	)
	
	::Display Manual at the beginning if no argument is inputted
	:displayManual
	echo Flash Magic Gang Programming
	echo.
	echo User Manual:
	echo 1. Connect LPC-Link2 debug boards to the computer
	echo     and NHS-3100 chips to the debug boards
	echo 2. Input the correct path of program to be flashed
	echo 3. Press Enter
	echo.
	
	::Prompt user's input
	:userInput
	echo Enter the program path ^(without ""^):
	echo ^(enter q to quit^)
	echo.
	set /p programPath=">>"
	
	::Check if q is inserted to quit the program
	:checkq
	if !programPath!==q (
		echo.
		echo Quit program.
		echo.
		goto :eof
	)
	
	::Check if the program path exists
	:checkPath
	if not exist !programPath! (
		echo.
		echo #################################################################
		echo MESSAGE:
		echo The following program path does not exist:
		echo !programPath!
		echo.
		::Jump back to input prompt if path is not exists
		goto :userInput
	)
		
	
::######################################################################################################################

	:: B: << TEXT FILES AND BATCH FILES CREATION >>
	
::######################################################################################################################
	
	:: description: 1. Create the desired batch file for every device connected.
	::				2. For every batch file:
	::					a. 6 execution steps are included and the status outputs are stored into the prog__.txt
	::					b. the value in check__.txt is being increment by 1 for every succeeded process. 
	::						This value is used to check if all the device has completed a specific process.
	
	::				**(For more details of flash__.bat: see "Sample_of_subbatch_file_created.bat"!)

	
	::Initialize index to 0 so that the first text file created will be "flash1.txt" after increment by 1
	set /a index=0

	::Create check__.txt with zero initiallized.
	::The value in check__.txt will be increased as the process is completed.
	::This value will later be continuously checked to ensure the flashing progress of all the devices connected be displayed correctly.
	::(This process will be explained further in the next section)
	echo 0 > checkDevice.txt & echo 0 > checkInterface.txt & echo 0 > checkErase.txt & echo 0 > checkHex.txt & echo 0 > checkVerify.txt & echo 0 > checkReset.txt
	
	::Create prog__.txt
	::prog__.txt will store the output of the respective flashing progress.
	echo. > progDevice.txt & echo. > progInterface.txt & echo. > progErase.txt & echo. > progHex.txt & echo. > progVerify.txt & echo. > progReset.txt

	::Display the starting time after user input
	echo. & echo.
	echo STARTING TIME:
	call :displayTime
	set /a startMin=!Min!
	set /a startSec=!Sec!
	echo Detecting device^(s^)...
	
	::Proceed only if checkDevice.txt has been created
	:checkDeviceExist
	if not exist checkDevice.txt (
		goto :checkDeviceExist
	)

	::For every LPC-Link2 debug board connected:
	::	1. get the interface serial number
	::	2. create dedicated batch file
	for /f %%g in ('USBMANAGER --seriallist --nobanner') do (
		
		::Increment index for every serial number detected to create equal number of flash__.bat
		set /a index=index+1
		
		echo @echo off > flash!index!.bat
		echo setlocal ENABLEDELAYEDEXPANSION >> flash!index!.bat
		echo. >>flash!index!.bat
		echo set /a task^=0 >> flash!index!.bat
		echo echo Processing... >> flash!index!.bat
		echo. >> flash!index!.bat
		
		echo for /f "delims=  skip=9" %%%%a in ^(^'FM DEVICE^^^(NHS3100^^^, 0^^^, 0^^^) INTERFACE^^^(SWDLINK2^^^, %%g^^^) ERASE^^^(DEVICE^^^,PROTECTISP^^^) HEXFILE^^^(%programPath%^^^, NOCHECKSUMS^^^, NOFILL^^^, PROTECTISP^^^) VERIFY^^^(%programPath%^^^, NOCHECKSUMS^^^) RESET^'^) do ^( >> flash!index!.bat
		
		echo echo %%%%a >> flash!index!.bat
		echo set /a task^=task+1 >> flash!index!.bat
		echo if ^^!task^^!^=^=1 ^( >> flash!index!.bat
		echo echo %%g ^^^>^^^> %%%%a ^>^> progDevice.txt >> flash!index!.bat
		echo. >> flash!index!.bat
		echo for /f %%%%a in ^(checkDevice.txt^) do ^( >> flash!index!.bat
		echo set /a checkDevice^=%%%%a >> flash!index!.bat
		echo set /a checkDevice^=checkDevice+1 >> flash!index!.bat
		echo echo ^^!checkDevice^^! ^> checkDevice.txt >> flash!index!.bat
		echo ^) >> flash!index!.bat
		echo ^) >> flash!index!.bat
		
		echo if ^^!task^^!^=^=2 ^( >> flash!index!.bat
		echo echo %%g ^^^>^^^> %%%%a ^>^> progInterface.txt >> flash!index!.bat
		echo. >> flash!index!.bat
		echo for /f %%%%a in ^(checkInterface.txt^) do ^( >> flash!index!.bat
		echo set /a checkInterface^=%%%%a >> flash!index!.bat
		echo set /a checkInterface^=checkInterface+1 >> flash!index!.bat
		echo echo ^^!checkInterface^^! ^> checkInterface.txt >> flash!index!.bat
		echo ^) >> flash!index!.bat
		echo ^) >> flash!index!.bat
		
		echo if ^^!task^^!^=^=3 ^( >> flash!index!.bat
		echo echo %%g ^^^>^^^> %%%%a ^>^> progErase.txt >> flash!index!.bat
		echo. >> flash!index!.bat
		echo for /f %%%%a in ^(checkErase.txt^) do ^( >> flash!index!.bat
		echo set /a checkErase^=%%%%a >> flash!index!.bat
		echo set /a checkErase^=checkErase+1 >> flash!index!.bat
		echo echo ^^!checkErase^^! ^> checkErase.txt >> flash!index!.bat
		echo ^) >> flash!index!.bat
		echo ^) >> flash!index!.bat

		echo if ^^!task^^!^=^=4 ^( >> flash!index!.bat
		echo echo %%g ^^^>^^^> %%%%a ^>^> progHex.txt >> flash!index!.bat
		echo. >> flash!index!.bat
		echo for /f %%%%a in ^(checkHex.txt^) do ^( >> flash!index!.bat
		echo set /a checkHex^=%%%%a >> flash!index!.bat
		echo set /a checkHex^=checkHex+1 >> flash!index!.bat
		echo echo ^^!checkHex^^! ^> checkHex.txt >> flash!index!.bat
		echo ^) >> flash!index!.bat
		echo ^) >> flash!index!.bat
		
		echo if ^^!task^^!^=^=5 ^( >> flash!index!.bat
		echo echo %%g ^^^>^^^> %%%%a ^>^> progVerify.txt >> flash!index!.bat
		echo. >> flash!index!.bat
		echo for /f %%%%a in ^(checkVerify.txt^) do ^( >> flash!index!.bat
		echo set /a checkVerify^=%%%%a >> flash!index!.bat
		echo set /a checkVerify^=checkVerify+1 >> flash!index!.bat
		echo echo ^^!checkVerify^^! ^> checkVerify.txt >> flash!index!.bat
		echo ^) >> flash!index!.bat
		echo ^) >> flash!index!.bat

		echo if ^^!task^^!^=^=6 ^( >> flash!index!.bat
		echo echo %%g ^^^>^^^> %%%%a ^>^> progReset.txt >> flash!index!.bat
		echo. >> flash!index!.bat
		echo for /f %%%%a in ^(checkReset.txt^) do ^( >> flash!index!.bat
		echo set /a checkReset^=%%%%a >> flash!index!.bat
		echo set /a checkReset^=checkReset+1 >> flash!index!.bat
		echo echo ^^!checkReset^^! ^> checkReset.txt >> flash!index!.bat
		::store the succeed interfaces
		echo echo %%g ^>^> interface.txt >> flash!index!.bat
		echo ^) >> flash!index!.bat
		echo ^) >> flash!index!.bat	
		echo ^) >> flash!index!.bat
	)
		
	::Add exit line to the bat files
	set /a writeExit=1
	:writeExit
	if !writeExit! leq !index! (
		echo exit >> flash!writeExit!.bat
		set /a writeExit=writeExit+1
		goto :writeExit
	)

::######################################################################################################################	

	:: C: << BATCH FILES EXECUTION >>

::######################################################################################################################	

	:: description: All flash__.bat starts simultaneously and involves 6 steps of execution:
	::				1. Device selection
	::				2. Interface selection
	::				3. Memory erase
	::				4. Program flashing
	::				5. Program verification
	::				6. Device reset
	
	::Jump to :message if no device is detected
	if !index!==0 (
		goto :message
	)
		
	::Execute all the batch files simultaneously
	set /a beginFlash=1
	:execute
	if !beginFlash! leq !index! (
		start flash!beginFlash!.bat
		set /a beginFlash=beginFlash+1
		goto :execute
	)

::######################################################################################################################

	:: D: << PROGRESS LOGGING >>

::######################################################################################################################	
	
	:: description: a. Sequentially from process to process, the main batch file continuously checks if the device check is fulfilled 
	::					and then displays the output before proceeding to the next step.
	::				b. For every failed process detected, the number of device check for the next process is reduced.
	::				c. Display process status.
	::				d. Lastly, it jumps to the message display when all the device flashing failed at any point of execution.
	
	::Initiallize checkReset to zero, so that the message section shows "0 flashing is completed" if no device is being flashed succesfully.
	Set /a checkReset=0
	
	
	::1) Device Detection
	::a. Loop to check if all devices are detected before continue
	:checkDevice
	for /f %%g in (checkDevice.txt) do (
		set /a checkDevice=%%g
	)
	::Index stores number of boards connected; checkDevice stores current number of device detected
	if not !checkDevice!==!index! (
		goto :checkDevice
	)
	::b. After all devices have been checked,
	::		deduct one checkDevice for every "failed:" detected from the output
	::		so that the next step will have fewer interface check
	for /f "tokens=4" %%a in (progDevice.txt) do (
		if %%a==failed: (
			set /a checkDevice=checkDevice-1
		)
	)
	::c. Display Device Connection status
	call :displayTitle DEVICE
	type progDevice.txt
	::d. Jump to message if all devices failed in this step
	if !checkDevice!==0 goto :message
	
	
	::2) Interface Selection
	::a. Loop to check if all interfaces are selected before continue
	:checkInterface
	for /f %%g in (checkInterface.txt) do (
		set /a checkInterface=%%g
	)
	::checkDevice stores number of interface serial number expected; checkInterface stores the current number of interface serial number selected
	if not !checkInterface!==!checkDevice! (
		goto :checkInterface
	)
	::b. After all interfaces have been set,
	::		deduct one checkInterface for every "failed:" detected from the output
	::		so that the next step will have fewer erasing
	for /f "tokens=4" %%a in (progInterface.txt) do (
		if %%a==failed: (
			set /a checkInterface=checkInterface-1
		)
	)
	::c. Display Interface Selection status
	call :displayTitle INTERFACE
	type progInterface.txt
	::d. Jump to message if all devices failed in this step
	if !checkInterface!==0 goto :message
	
	
	::3) Erasing Memory
	::a. Loop to check if all erase is completed before continue
	:checkErase
	for /f %%g in (checkErase.txt) do (
		set /a checkErase=%%g
	)
	::checkInterface stores number of erasing expected; checkErase stores the current number of erasing completed
	if not !checkErase!==!checkInterface! (
		goto :checkErase
	)
	::b. After all erase is done,
	::		deduct one checkErase for every "failed:" detected from the output
	::		so that the next step will have fewer flashing
	for /f "tokens=4" %%a in (progErase.txt) do (
		if %%a==failed: (
			set /a checkErase=checkErase-1
		)
	)
	::c. Display Erasing status
	call :displayTitle ERASING
	type progErase.txt
	::d. Jump to message if all devices failed in this step
	if !checkErase!==0 goto :message
	
	::4) Flashing program
	::a. Loop to check if all flashing is completed before continue
	:checkHex
	for /f %%g in (checkHex.txt) do (
		set /a checkHex=%%g
	)
	::checkErase stores number of flashing expected; checkHex stores the current number of flashing completed
	if not !checkHex!==!checkErase! (
		goto :checkHex
	)
	::b. After all flashing is done,
	::		deduct one checkHex for every "failed:" detected from the output
	::		so that the next step will have fewer verification
	for /f "tokens=6" %%a in (progHex.txt) do (
		if %%a==failed: (
			set /a checkHex=checkHex-1
		)
	)
	::c. Display Flashing status
	call :displayTitle FLASHING
	type progHex.txt	
	::d. Jump to message if all devices failed in this step
	if !checkHex!==0 goto :message
	
	
	::5) Verification
	::a. Loop to check if all verification is completed before continue
	:checkVerify
	for /f %%g in (checkVerify.txt) do (
		set /a checkVerify=%%g
	)
	::checkHex stores number of verification expected; checkVerify stores the current number of verification completed
	if not !checkVerify!==!checkHex! (
		goto :checkVerify
	)
	::b. After all verification is done,
	::		deduct one checkVerify for every "failed:" detected from the output
	::		so that the next step will have fewer reset
	for /f "tokens=4" %%a in (progVerify.txt) do (
		if %%a==failed: (
			set /a checkVerify=checkVerify-1
		)
	)
	::c. Display Verification status
	call :displayTitle VERIFICATION
	type progVerify.txt
	::d. Jump to message if all devices failed in this step
	if !checkVerify!==0 goto :message
	
	
	::6) Reset
	::a. Loop to check if all reset is completed before continue
	:checkReset
	for /f %%g in (checkReset.txt) do (
		set /a checkReset=%%g
	)
	::checkVerify stores number of reset expected; checkReset stores the current number of reset completed
	if not !checkReset!==!checkVerify! (
		goto :checkReset
	)
	::c.Display Reset status
	call :displayTitle RESET
	type progReset.txt
	
	::Display end time
	echo. & echo.
	echo END TIME:
	call :displayTime
	
	::Display elapsed time
	echo.
	::Elapsed time calculation (EndMin-StartMin)*60+(EndSec-StartSec)
	set /a "elap=(!Min!-!startMin!)*60+!Sec!-!startSec!"
	echo ELAPSED TIME: !elap! seconds


::######################################################################################################################	

	:: E: << RESULT MESSAGES >>	

::######################################################################################################################	
	
	:: description: 1. If no device is detected, display message and jump to :delete
	::				2. If flashing is completed, display number of suceeded and failed flashing
	::				3. If there is at least 1 flashing completed, 
	::						display the program path flashed and the all the interface serial number of succeeded flashing
	
	:message
	echo. & echo.
	echo ###########################################################################
	echo MESSAGE:
	
	::This message displayed if no device is detected
	::Index stores the number of LPC-Link2 board detected
	if !index! == 0 (
		echo.
		echo ###################################################################
		echo No target device is detected. Please check your device connections.
		echo ###################################################################
		echo.
		echo.
		goto :delete
	)

	::Display the number of succeeded flashing
	::checkReset stores the final number of completed flashing
	::For 0 and 1 succeeded flashing
	if !checkReset! lss 2 (
		echo !checkReset! flashing is completed^^!
	) else (
		::For 2 or more succeeded flashings
		if not !checkReset!==!index! (
			echo !checkReset! flashings are completed^^!
		) else (
		::Display "All ..." if all the detected devices has the flashing completed
			echo All !checkReset! flashings are completed^^!
		)
	)
	
	::Display the number of failed flashing
	if not !checkReset!==!index! (
		set /a fail=index-checkReset
		echo !fail! failed attempt^(s^).
	)
	
	::Skip displaying program path and interface serial number if no flashing is completed
	echo.
	if !checkInterface!==0 goto :delete
	if !checkErase!==0 goto :delete
	if !checkHex!==0 goto :delete
	if !checkReset!==0 goto :delete
	
	::Display path of program flashed
	echo Flashed program path:
	echo !programPath!
	
	::Display interface serial number of succeeded flashing
	echo.
	echo Interface serial number of succeeded flashing:
	type interface.txt
	echo.
	echo ^(Scroll up for more information^)
	echo.
	
::######################################################################################################################

	:: F: << TEXT FILES AND BATCH FILES DELETION >>
	
::######################################################################################################################	

	:: description: Delete all the created text files and batch files to keep the folder clean.
	
	del interface.txt
	
	:delete
	::Delete check__.txt
	del checkDevice.txt & del checkInterface.txt & del checkErase.txt & del checkHex.txt & del checkVerify.txt & del checkReset.txt
	
	::Delete prog__.txt
	del progDevice.txt & del progInterface.txt & del progErase.txt & del progHex.txt & del progVerify.txt & del progReset.txt

	::Delete flash__.bat
	set /a delete=1
	
		:loop
		if !delete! leq !index! (
			del flash!delete!.bat
			set /a delete=delete+1
			goto :loop
		)	
	
	::Check if program path is pass using argument
	::If not true, pause the program before exit
	if !arg!==0 (
		pause
	)

:eof	
endlocal
