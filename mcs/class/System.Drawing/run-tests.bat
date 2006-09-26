@echo off
REM ********************************************************
REM This batch call all the test batches with default parameters
REM ********************************************************
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM ********************************************************

IF "%1"=="" (
	set BUILD_OPTION=rebuild
) ELSE (
	set BUILD_OPTION=%1
)
	
IF "%2"=="" (
	set OUTPUT_FILE_PREFIX=System_Drawing_MonoTests
) ELSE (
	set OUTPUT_FILE_PREFIX=%2
)

	
IF "%3"=="" (
	set RUNNING_FIXTURE=MonoTests
) ELSE (
	set RUNNING_FIXTURE=%3
)

call run-tests.test.bat %BUILD_OPTION% %OUTPUT_FILE_PREFIX% %RUNNING_FIXTURE% "" ""

IF "%1"=="" (
	set BUILD_OPTION=rebuild
) ELSE (
	set BUILD_OPTION=%1
)
	
IF "%2"=="" (
	set OUTPUT_FILE_PREFIX=System_Drawing_Test
) ELSE (
	set OUTPUT_FILE_PREFIX=%2
)

	
IF "%3"=="" (
	set RUNNING_FIXTURE=Test
) ELSE (
	set RUNNING_FIXTURE=%3
)

@echo on
call run-tests.test.bat %BUILD_OPTION% %OUTPUT_FILE_PREFIX% %RUNNING_FIXTURE% "Test\DrawingTest\Test" "..\..\..\"