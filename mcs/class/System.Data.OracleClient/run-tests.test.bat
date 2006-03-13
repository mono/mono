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


IF "%1"=="" (
	set BUILD_OPTION=rebuild
) ELSE (
	set BUILD_OPTION=%1
)

REM ********************************************************
REM Set parameters
REM ********************************************************

set BUILD_OPTION=%1
set OUTPUT_FILE_PREFIX=MonoTests.System.Data.OracleClient
set RUNNING_FIXTURE=MonoTests.System.Data.OracleClient

set TEST_SOLUTION=Test\System.Data.OracleClient.Tests.J2EE.sln
set TEST_ASSEMBLY=System.Data.OracleClient.Tests.J2EE.jar
set PROJECT_CONFIGURATION=Debug_Java
set APP_CONFIG_FILE=Test\System.Data.OracleClient.J2EE.config

set OUTPUT_FILE_PREFIX=%OUTPUT_FILE_PREFIX%

REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%GH_HOME%\jgac\vmw4j2ee_110\

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Xml.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Data.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%J2SE.Helpers.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Data.OracleClient.jar

set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%GH_HOME%\jgac\jdbc\ojdbc14.jar

set NUNIT_OPTIONS=/exclude=NotWorking

set NET_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.Net.xml
set GH_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.GH.xml

set NUNIT_PATH=..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.framework.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.util.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit.core.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\Debug_Java\nunit-console.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;.
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%TEST_ASSEMBLY%

set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"

REM ********************************************************
@echo Building GH solution...
REM ********************************************************

devenv %TEST_SOLUTION% /%BUILD_OPTION% %PROJECT_CONFIGURATION% >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Building NUnit solution...
REM ********************************************************

devenv ..\..\nunit20\nunit.java.sln /%BUILD_OPTION% %PROJECT_CONFIGURATION% >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Running GH tests...
REM ********************************************************

REM ********************************************************
@echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

copy Test\bin\%PROJECT_CONFIGURATION%\%TEST_ASSEMBLY% .
copy %APP_CONFIG_FILE% nunit-console.exe.config


REM @echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUNNING_FIXTURE%_run.log.txt 2<&1
REM @echo off

REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool
devenv %XML_TOOL_PATH%\XmlTool.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

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
@echo Parameters: "[build|rebuild]"
GOTO END

:END
REM EXIT 0
