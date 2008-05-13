REM @echo on
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
set OUTPUT_FILE_PREFIX=System_Xml_Linq_MonoTests
set RUNNING_FIXTURE=MonoTests.System.Xml.Linq
set TEST_PROJECT=Test\System.Xml.Linq.Tests.csproj
set TEST_ASSEMBLY=System.Xml.Linq.Tests.jar
set PROJECT_CONFIGURATION=Debug_Java


set startDate=%date%
set startTime=%time%
set sdy=%startDate:~10%
set /a sdm=1%startDate:~4,2% - 100
set /a sdd=1%startDate:~7,2% - 100
set /a sth=%startTime:~0,2%
set /a stm=1%startTime:~3,2% - 100
set /a sts=1%startTime:~6,2% - 100
set TIMESTAMP=%sdy%_%sdm%_%sdd%_%sth%_%stm%


REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%VMW_HOME%\java_refs\framework\
set JAVA_HOME=%VMW_HOME%\jre

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Xml.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Configuration.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%J2SE.Helpers.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Drawing.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Xml.Linq.jar
set NUNIT_OPTIONS=/exclude=NotWorking,NotDotNet,TargetJvmNotSupported,TargetJvmNotWorking,NotDotNet

if "%GH_VERSION%"=="" (
	set GH_VERSION=0_0_0_0
)

set COMMON_PREFIX=%TIMESTAMP%_%OUTPUT_FILE_PREFIX%.GH_%GH_VERSION%.1.%USERNAME%
set GH_OUTPUT_XML=%COMMON_PREFIX%.xml
set BUILD_LOG=%COMMON_PREFIX%.build.log
set RUN_LOG=%COMMON_PREFIX%.run.log

set OUTPUT_DIR=..\..
set NUNIT_PATH=..\..\nunit20
set NUNIT_CONFIGURATION=Debug_Java20
set NUNIT_CLASSPATH=%NUNIT_PATH%\nunit-console\bin\%NUNIT_CONFIGURATION%\nunit.framework.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%\nunit-console\bin\%NUNIT_CONFIGURATION%\nunit.util.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%\nunit-console\bin\%NUNIT_CONFIGURATION%\nunit.core.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%\nunit-console\bin\%NUNIT_CONFIGURATION%\nunit-console.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;.
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%TEST_ASSEMBLY%

set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"

REM ********************************************************
@echo Building GH solution...
REM ********************************************************

msbuild %TEST_PROJECT% /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
rem @echo Building NUnit solution...
REM ********************************************************

if "%NUNIT_BUILD%" == "DONE" goto NUNITSKIP

REM devenv ..\..\..\nunit20\nunit.java.sln /%BUILD_OPTION% %PROJECT_CONFIGURATION% >>%RUNNING_FIXTURE%_build.log.txt 2<&1
rem msbuild %NUNIT_PATH%nunit20.java.sln /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

goto NUNITREADY

:NUNITSKIP
echo Skipping NUnit Build...

:NUNITREADY
set NUNIT_BUILD=DONE

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Running GH tests...
REM ********************************************************

REM ********************************************************
@echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

copy Test\bin\%PROJECT_CONFIGURATION%\%TEST_ASSEMBLY% .

set TEST_TEMP=C:\MonoTemp
mkdir %TEST_TEMP%

REM @echo on
echo "%JAVA_HOME%\bin\java" -server -Xmx1024M -Djava.io.tmpdir="%TEST_TEMP%" -classpath %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% 
"%JAVA_HOME%\bin\java" -server -Xmx1024M -Djava.io.tmpdir="%TEST_TEMP%" -classpath %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUN_LOG% 2<&1
REM jdb -server -Xmx1024M -Djava.io.tmpdir="%TEST_TEMP%" -classpath %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML%
REM @echo off

REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool

if "%XMLTOOL_BUILD%" == "DONE" goto XMLTOOLSKIP

REM devenv %XML_TOOL_PATH%\XmlTool.sln /%BUILD_OPTION% %PROJECT_CONFIGURATION% >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild %XML_TOOL_PATH%\XmlTool20.csproj /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

goto XMLTOOLREADY

:XMLTOOLSKIP
echo Skipping XmlToll build...

:XMLTOOLREADY
set XMLTOOL_BUILD=DONE

copy %XML_TOOL_PATH%\bin\%PROJECT_CONFIGURATION%\xmltool.exe .
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
@echo This test requires environment variable VMW_HOME to be defined
GOTO END

:BUILD_EXCEPTION
@echo Error in building solutions. See %BUILD_LOG% for details...
REM EXIT 1
GOTO END

:RUN_EXCEPTION
@echo Error in running fixture %RUNNING_FIXTURE%. See %RUN_LOG% for details...
REM EXIT 1
GOTO END

:USAGE
@echo Parameters: "[build|rebuild] <output_file_name_prefix> <test_fixture> <relative_Working_directory> <back_path (..\..\.....) >"
GOTO END

:END
copy %RUN_LOG% %OUTPUT_DIR%
copy %BUILD_LOG% %OUTPUT_DIR%
copy %GH_OUTPUT_XML% %OUTPUT_DIR%

REM EXIT 0
