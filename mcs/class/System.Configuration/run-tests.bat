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

set OUTPUT_FILE_PREFIX=MonoTests.System.DirectoryServices
set RUNNING_FIXTURE=MonoTests.System.Configuration

call run-tests.test.bat %BUILD_OPTION% %SECURE_MODE% %OUTPUT_FILE_PREFIX% %RUNNING_FIXTURE%
