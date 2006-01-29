@echo off
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM output files name prefix (mandratory) : prefix for naming output xml files
REM test fixture name (optional) : if you want to run some particular test fixture
REM directory to run tests (optional)
REM path back to root directory (opposite to previous param)
REM example run-tests build GhTests Test.Sys.Drawing Test\DrawingTest\Test ..\..\..\
REM will cause to build (and not rebuild) test solutions,
REM running Test.Sys.Drawing fixture in directory Test\DrawingTest\Test
REM with output files named GhTests.Net.xml and GhTests.GH.xml
REM ********************************************************

IF "%1"=="" GOTO USAGE

IF "%JAVA_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION

IF "%GH_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION

REM ********************************************************
REM Set parameters
REM ********************************************************

set BUILD_OPTION=%1
set OUTPUT_FILE_PREFIX=%2
set RUNNING_FIXTURE=%3
set RUNNING_DIR=%~4
set BACK_TO_ROOT_DIR=%~5


REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%GH_HOME%\jgac\vmw4j2ee_110\

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar;%JGAC_PATH%System.jar;%JGAC_PATH%System.Xml.jar;%JGAC_PATH%System.Drawing.jar;%JGAC_PATH%J2SE.Helpers.jar;%JGAC_PATH%jai_imageio.jar
set NUNIT_OPTIONS=/exclude=NotWorking

set NET_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.Net.xml
set GH_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.GH.xml

set NUNIT_PATH=%BACK_TO_ROOT_DIR%..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.framework.jar;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.util.jar;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.core.jar;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit-console.jar
set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"


REM ********************************************************
@echo Building .Net solution...
REM ********************************************************

devenv Test\DrawingTest\System.Drawing.Test.dotnet.sln /%BUILD_OPTION% Debug >%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Building GH solution...
REM ********************************************************

devenv Test\DrawingTest\System.Drawing.Test.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Building NUnit solution...
REM ********************************************************

devenv ..\..\nunit20\nunit.java.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Running Net reference tests...
REM ********************************************************

REM ********************************************************
@echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

if "%RUNNING_DIR%" NEQ "" (
	cd %RUNNING_DIR% )

if not exist Exocortex.DSP.v1.dll (
	copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\Exocortex.DSP.v1.dll .)

if not exist DrawingTest.dll (
	copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\DrawingTest.dll . )

if not exist Test.dotnet.dll (
	copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\Test.dotnet.dll . )

copy "%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit-console.exe" .
copy "%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.util.dll" .
copy "%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.core.dll" .
copy "%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.framework.dll" .
nunit-console.exe /fixture:%RUNNING_FIXTURE% Test.dotnet.dll %NUNIT_OPTIONS% /xml=%NET_OUTPUT_XML% >%RUNNING_FIXTURE%_run.log.txt 2<&1


REM ********************************************************
@echo Running GH tests...
REM ********************************************************

REM ********************************************************
@echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\bin\Debug_Java\Exocortex.DSP.v1.jar .
copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\bin\Debug_Java\DrawingTest.jar .
copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\bin\Debug_Java\Test.jar .


REM @echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi Test.jar /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUNNING_FIXTURE%_run.log.txt 2<&1
REM @echo off

if "%RUNNING_DIR%" NEQ "" (
	copy %NET_OUTPUT_XML% %BACK_TO_ROOT_DIR%
	copy %GH_OUTPUT_XML% %BACK_TO_ROOT_DIR%
	cd %BACK_TO_ROOT_DIR% )

REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\xmltool
devenv %XML_TOOL_PATH%\XmlTool.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

copy %XML_TOOL_PATH%\bin\Debug_Java\xmltool.exe .
copy %XML_TOOL_PATH%\nunit_transform.xslt .

REM ********************************************************
@echo Analyze and print results
REM ********************************************************
@echo on
xmltool.exe --transform nunit_transform.xslt %GH_OUTPUT_XML%
@echo off

:FINALLY
GOTO END

:ENVIRONMENT_EXCEPTION
@echo This test requires environment variables JAVA_HOME and GH_HOME to be defined
GOTO END

:BUILD_EXCEPTION
@echo Error in building solutions. See %RUNNING_FIXTURE%_build.log.txt for details...
REM EXIT 1
GOTO END

:RUN_EXCEPTION
@echo Error in running fixture %RUNNING_FIXTURE%. See %RUNNING_FIXTURE%_run.log.txt for details...
REM EXIT 1
GOTO END

:USAGE
@echo Parameters: "[build|rebuild] <output_file_name_prefix> <test_fixture> <relative_Working_directory> <back_path (..\..\.....) >"
GOTO END

:END
REM EXIT 0
