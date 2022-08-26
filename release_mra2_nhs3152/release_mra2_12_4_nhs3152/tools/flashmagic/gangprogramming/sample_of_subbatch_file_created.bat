@rem Copyright 2017 NXP
@rem This software is owned or controlled by NXP and may only be used strictly
@rem in accordance with the applicable license terms.  By expressly accepting
@rem such terms or by downloading, installing, activating and/or otherwise using
@rem the software, you are agreeing that you have read, and that you agree to
@rem comply with and are bound by, such license terms.  If you do not agree to
@rem be bound by the applicable license terms, then you may not retain, install,
@rem activate or otherwise use the software.

:: Description: 1. Number of batch file created will be the same as the number of LPC-Link2 board connected.
::				2. For every task of execution:
::					a. the task status is stored into prog__.txt
::					b. the value in check__.txt is incremented (update the number of current device that has the respective task completed)
@echo This is a sample batch file and is intended for code inspection only.
@exit

@echo off
setlocal ENABLEDELAYEDEXPANSION 

::Initialize task with 0
set /a task=0
echo Processing...

::Execute the 6 tasks including DEVICE, INTERFACE, ERASE, HEXFILE, VERIFY, RESET
::After task execution, the text of the output is obtained.
::With the output text, the batch file executes the lines in the for loop.

::First, it skips the first 9 lines of the output text. (Skip the Flash Magic program description)
::Then, it starts with the consecutive line till the end of the output, getting one line at a time.
::Every line is a separate task.
::Therefore, the section of "Device Detection" will be executed for the first output text line, following by "Interface Selection" the next output text line, and so on.
for /f "delims=  skip=9" %%a in ('FM DEVICE^(NHS3100^, 0^, 0^) INTERFACE^(SWDLINK2^, NqG9H7spLxk5^) ERASE^(DEVICE^,PROTECTISP^) HEXFILE^(C:\path\to\firmware.HEX^, NOCHECKSUMS^, NOFILL^, PROTECTISP^) VERIFY^(C:\path\to\firmware.HEX^, NOCHECKSUMS^) RESET') do ( 
	
	::Display the current status of execution
	echo %%a
	::Increment task after executing every task
	set /a task=task+1
	
	::1. Device Detection
	if !task!==1 (
		::Store Device Detection status into progDevice.txt
		echo NqG9H7spLxk5 ^>^> %%a >> progDevice.txt 
		::Increment the value in checkDevice.txt
		for /f %%a in (checkDevice.txt) do ( 
			set /a checkDevice=%%a 
			set /a checkDevice=checkDevice+1 
			echo !checkDevice! > checkDevice.txt 
		)
	)

	::2. Interface Selection
	if !task!==2 ( 
		::Store Interface Selection status into progInterface.txt
		echo NqG9H7spLxk5 ^>^> %%a >> progInterface.txt 
		::Increment the value in checkInterface.txt
		for /f %%a in (checkInterface.txt) do ( 
			set /a checkInterface=%%a 
			set /a checkInterface=checkInterface+1 
			echo !checkInterface! > checkInterface.txt 
		) 
	) 
	
	::3. Erasing memory
	if !task!==3 (
		::Store Erasing status into progErase.txt
		echo NqG9H7spLxk5 ^>^> %%a >> progErase.txt 
		::Increment the value in checkErase.txt
		for /f %%a in (checkErase.txt) do ( 
			set /a checkErase=%%a 
			set /a checkErase=checkErase+1 
			echo !checkErase! > checkErase.txt 
		) 
	)
	
	::4. Flashing program
	if !task!==4 ( 
		::Store Flashing status into progHex.txt
		echo NqG9H7spLxk5 ^>^> %%a >> progHex.txt 
		::Increment the value in checkHex.txt
		for /f %%a in (checkHex.txt) do ( 
			set /a checkHex=%%a 
			set /a checkHex=checkHex+1 
			echo !checkHex! > checkHex.txt 
		) 
	) 
	
	::5. Verification of program
	if !task!==5 ( 
		::Store verification status into progVerify.txt
		echo NqG9H7spLxk5 ^>^> %%a >> progReset.txt 
		::Increment the value in checkVerify.txt
		for /f %%a in (checkReset.txt) do ( 
			set /a checkReset=%%a 
			set /a checkReset=checkReset+1 
			echo !checkReset! > checkReset.txt 
			echo NqG9H7spLxk5 >> interface.txt 
		) 
	)
	
	::6. Reset
	if !task!==6 ( 
		::Store Reset status into progReset.txt
		echo NqG9H7spLxk5 ^>^> %%a >> progReset.txt 
		::Increment the value in checkReset.txt
		for /f %%a in (checkReset.txt) do ( 
			set /a checkReset=%%a 
			set /a checkReset=checkReset+1 
			echo !checkReset! > checkReset.txt 
			::Store succeeded interface into interface.txt
			echo NqG9H7spLxk5 >> interface.txt 
		) 
	) 	
) 
exit
