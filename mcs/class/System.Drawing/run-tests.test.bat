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

IF "%VMW_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION

REM ********************************************************
REM Set parameters
REM ********************************************************

set BUILD_OPTION=%1
set OUTPUT_FILE_PREFIX=%2
set RUNNING_FIXTURE=%3
set RUNNING_DIR=%~4
set BACK_TO_ROOT_DIR=%~5

set TEST_J2EE_SOLUTION=Test\System.Drawing.Test20.J2EE.sln
set TEST_NET_SOLUTION=Test\System.Drawing.Test20.sln
set TEST_J2EE_ASSEMBLY=System.Drawing.Test20.J2EE.jar
set TEST_NET_ASSEMBLY=System.Drawing.Test.dll
set PROJECT_J2EE_CONFIGURATION=Debug_Java20
set PROJECT_NET_CONFIGURATION=Debug
set NUNIT_CONSOLE_PATH="C:\Program Files\NUnit-Net-2.0 2.2.8\bin"
set JAVA_HOME=%VMW_HOME%\jre5

set DATEL=%date:~4,2%_%date:~7,2%_%date:~10,4%
set TIMEL=%time:~0,2%_%time:~3,2%
set TIMESTAMP=%DATEL%_%TIMEL%

REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%VMW_HOME%\jgac\vmw4j2ee_110\

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar;%JGAC_PATH%System.jar;%JGAC_PATH%System.Xml.jar;%JGAC_PATH%System.Drawing.jar;%JGAC_PATH%J2SE.Helpers.jar;%JGAC_PATH%jai_imageio.jar
set NUNIT_OPTIONS=/exclude=NotWorking

set NET_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.%RUNNING_FIXTURE%.Net.%TIMESTAMP%.xml
set GH_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.%RUNNING_FIXTURE%.GH.%TIMESTAMP%.xml

set BUILD_LOG=%OUTPUT_FILE_PREFIX%.%RUNNING_FIXTURE%_build.log.%TIMESTAMP%.txt
set RUN_LOG=%OUTPUT_FILE_PREFIX%.%RUNNING_FIXTURE%_run.log.%TIMESTAMP%.txt

set NUNIT_PATH=%BACK_TO_ROOT_DIR%..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\%PROJECT_J2EE_CONFIGURATION%\nunit.framework.jar;%NUNIT_PATH%nunit-console\bin\%PROJECT_J2EE_CONFIGURATION%\nunit.util.jar;%NUNIT_PATH%nunit-console\bin\%PROJECT_J2EE_CONFIGURATION%\nunit.core.jar;%NUNIT_PATH%nunit-console\bin\%PROJECT_J2EE_CONFIGURATION%\nunit-console.jar
set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"


REM ********************************************************
@echo Building .Net solution...
REM ********************************************************

REM devenv Test\DrawingTest\System.Drawing.Test.dotnet.sln /%BUILD_OPTION% Debug >%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild %TEST_NET_SOLUTION% /t:%BUILD_OPTION% /p:Configuration=%PROJECT_NET_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION



if "%NUNIT_BUILD%" == "DONE" goto NUNITSKIP

REM ********************************************************
@echo Building NUnit solution...
REM ********************************************************

REM devenv ..\..\nunit20\nunit.java.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild ..\..\nunit20\nunit20.java.sln /t:%BUILD_OPTION% /p:Configuration=%PROJECT_J2EE_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

goto NUNITREADY

:NUNITSKIP
echo Skipping NUnit Build...

:NUNITREADY
set NUNIT_BUILD=DONE

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Building GH solution...
REM ********************************************************

REM devenv Test\DrawingTest\System.Drawing.Test.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild %TEST_J2EE_SOLUTION% /t:%BUILD_OPTION% /p:Configuration=%PROJECT_J2EE_CONFIGURATION% >>%BUILD_LOG% 2<&1

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

if not exist %TEST_NET_ASSEMBLY% (
	copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\%TEST_NET_ASSEMBLY% . )

copy "%NUNIT_PATH%nunit-console\bin\%PROJECT_J2EE_CONFIGURATION%\nunit-console.exe" .
copy "%NUNIT_PATH%nunit-console\bin\%PROJECT_J2EE_CONFIGURATION%\nunit.util.dll" .
copy "%NUNIT_PATH%nunit-console\bin\%PROJECT_J2EE_CONFIGURATION%\nunit.core.dll" .
copy "%NUNIT_PATH%nunit-console\bin\%PROJECT_J2EE_CONFIGURATION%\nunit.framework.dll" .
%NUNIT_CONSOLE_PATH%\nunit-console.exe /fixture:%RUNNING_FIXTURE% %TEST_NET_ASSEMBLY% %NUNIT_OPTIONS% /xml=%NET_OUTPUT_XML% >>%RUN_LOG% 2<&1


REM ********************************************************
@echo Running GH tests...
REM ********************************************************

REM ********************************************************
@echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\bin\%PROJECT_J2EE_CONFIGURATION%\Exocortex.DSP.v1.jar .
copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\bin\%PROJECT_J2EE_CONFIGURATION%\DrawingTest.jar .
copy %BACK_TO_ROOT_DIR%Test\DrawingTest\Test\bin\%PROJECT_J2EE_CONFIGURATION%\%TEST_J2EE_ASSEMBLY% .


REM @echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi %TEST_J2EE_ASSEMBLY% /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUN_LOG% 2<&1
REM @echo off

if "%RUNNING_DIR%" NEQ "" (
	copy %NET_OUTPUT_XML% %BACK_TO_ROOT_DIR%
	copy %GH_OUTPUT_XML% %BACK_TO_ROOT_DIR%
	cd %BACK_TO_ROOT_DIR% )

REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool
REM devenv %XML_TOOL_PATH%\XmlTool.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild %XML_TOOL_PATH%\XmlTool20.vmwcsproj /t:%BUILD_OPTION% /p:Configuration=%PROJECT_J2EE_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

copy %XML_TOOL_PATH%\bin\%PROJECT_J2EE_CONFIGURATION%\xmltool.exe .
copy %XML_TOOL_PATH%\nunit_transform.xslt .

REM ********************************************************
@echo Analyze and print results
REM ********************************************************
@echo on
xmltool.exe --transform nunit_transform.xslt %GH_OUTPUT_XML%
@echo off

:FINALLY

copy %RUNNING_DIR%\%RUN_LOG% ..\%BACK_TO_ROOT_DIR%
copy %BUILD_LOG% ..\%BACK_TO_ROOT_DIR%
copy %NET_OUTPUT_XML% ..\%BACK_TO_ROOT_DIR%
copy %GH_OUTPUT_XML% ..\%BACK_TO_ROOT_DIR%

GOTO END

:ENVIRONMENT_EXCEPTION
@echo This test requires environment variable VMW_HOME to be defined
GOTO END

:BUILD_EXCEPTION
@echo Error in building solutions. See System.Drawing.%RUNNING_FIXTURE%_build.log.txt for details...
REM EXIT 1
GOTO END

:RUN_EXCEPTION
@echo Error in running fixture %RUNNING_FIXTURE%. See System.Drawing.%RUNNING_FIXTURE%_run.log.txt for details...
REM EXIT 1
GOTO END

:USAGE
@echo Parameters: "[build|rebuild] <output_file_name_prefix> <test_fixture> <relative_Working_directory> <back_path (..\..\.....) >"
GOTO END

:END
REM EXIT 0
