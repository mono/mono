rem @echo off
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM example run-tests build 
REM will cause to build (and not rebuild) test solutions,
REM ********************************************************

IF "%JAVA_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION

IF "%VMW_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION
IF "%GHROOT%"=="" set GHROOT=%VMW_HOME%

REM ********************************************************
REM Set parameters
REM ********************************************************

IF "%1"=="" (
	set BUILD_OPTION=rebuild
) ELSE (
	set BUILD_OPTION=%1
)
set OUTPUT_FILE_PREFIX=GH_TEST


REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%VMW_HOME%\jgac\vmw4j2ee_110
set RUNTIME_CLASSPATH=%JGAC_PATH%\mscorlib.jar;%JGAC_PATH%\System.jar;%JGAC_PATH%\System.Xml.jar;%JGAC_PATH%\J2SE.Helpers.jar;
set NUNIT_OPTIONS=/exclude=NotWorking,CAS,InetAccess
set PROJECT_CONFIGURATION=Debug_Java20
set GH_OUTPUT_XML=nunit_results.xml
set NUNIT_PATH=..\..\nunit20
set NUNIT_CLASSPATH=%NUNIT_PATH%\nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.framework.jar;%NUNIT_PATH%\nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.util.jar;%NUNIT_PATH%\nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.core.jar;%NUNIT_PATH%\nunit-console\bin\%PROJECT_CONFIGURATION%\nunit-console.jar
set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"

REM ********************************************************
@echo Building NUnit solution...
REM ********************************************************

if "%NUNIT_BUILD%" == "DONE" goto NUNITSKIP
msbuild %NUNIT_PATH%\nunit20.java.sln /t:%BUILD_OPTION% /p:configuration=%PROJECT_CONFIGURATION% >build.log.txt 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

set NUNIT_BUILD=DONE

:NUNITSKIP
echo Skipping NUnit Build...



REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool
msbuild %XML_TOOL_PATH%\XmlTool20.sln /p:configuration=Debug >>build.log.txt 2<&1
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION
copy %XML_TOOL_PATH%\bin\Debug\xmltool20.exe .
copy %XML_TOOL_PATH%\nunit_transform.xslt .

REM ********************************************************
@echo Building GH solution...
REM ********************************************************
msbuild System-tests20.sln /t:%BUILD_OPTION% /p:configuration=%PROJECT_CONFIGURATION% >>build.log.txt 2<&1
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Running GH tests...
REM ********************************************************
@echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi /xml=%GH_OUTPUT_XML% bin\%PROJECT_CONFIGURATION%\System_tests.jar %NUNIT_OPTIONS%   >run.log.txt 2<&1
@echo off

REM ********************************************************
@echo Analyze and print results
REM ********************************************************
@echo on
xmltool20.exe --transform nunit_transform.xslt %GH_OUTPUT_XML%
@echo off

:FINALLY
GOTO END

:ENVIRONMENT_EXCEPTION
@echo This test requires environment variables JAVA_HOME and VMW_HOME to be defined
GOTO END

:BUILD_EXCEPTION
popd
@echo Error in building solutions. See build.log.txt for details...
GOTO END

:RUN_EXCEPTION
popd
@echo Error in running fixture. See run.log.txt for details...
GOTO END

:END
