@echo oî
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM example run-tests build 
REM will cause to build (and not rebuild) test solutions,
REM ********************************************************

IF "%VMW_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION



IF "%1"=="" (
	set BUILD_OPTION=build
) ELSE (
	set BUILD_OPTION=%1
)

REM ********************************************************
REM Set parameters
REM ********************************************************

set OUTPUT_FILE_PREFIX=System.Web
set RUNNING_FIXTURE=System.Web
set TEST_SOLUTION=Test\TestMonoWeb.J2EE20.sln
set TEST_ASSEMBLY=TestMonoWeb.jar
set PROJECT_CONFIGURATION=Debug_Java20


set DATEL=%date:~10,4%_%date:~4,2%_%date:~7,2%%
set TIMEL=%time:~0,2%_%time:~3,2%
set TIMESTAMP=%DATEL%_%TIMEL%


REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%VMW_HOME%\jgac\vmw4j2ee_110\
set JAVA_HOME=%VMW_HOME%\jre5

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Xml.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Web.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Runtime.Serialization.Formatters.Soap.jar
rem set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Drawing.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%J2SE.Helpers.jar

set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;Test\mainsoft\NunitWeb\NunitWeb\bin\Debug_Java20\NunitWeb.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;Test\mainsoft\NunitWeb\NunitWeb\bin\Debug_Java20\HtmlCompare.jar
set NUNIT_OPTIONS=/exclude=NotWorking

set GH_OUTPUT_XML=%TIMESTAMP%.%OUTPUT_FILE_PREFIX%.GH.xml
set BUILD_LOG=%TIMESTAMP%.%OUTPUT_FILE_PREFIX%.GH.%RUNNING_FIXTURE%.build.log
set RUN_LOG=%TIMESTAMP%.%OUTPUT_FILE_PREFIX%.GH.%RUNNING_FIXTURE%.run.log

set NUNIT_PATH=..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.framework.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.util.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.core.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit-console.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;.
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%TEST_ASSEMBLY%

set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"

REM ********************************************************
@echo Building GH solution...
REM ********************************************************

msbuild %TEST_SOLUTION% /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

IF "%BUILD_OPTION%"=="nobuild" GOTO RUN

REM ********************************************************
@echo Building NUnit solution...
REM ********************************************************

if "%NUNIT_BUILD%" == "DONE" goto NUNITSKIP
msbuild %NUNIT_PATH%\nunit20.java.sln /t:%BUILD_OPTION% /p:configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

goto NUNITREADY

:NUNITSKIP
echo Skipping NUnit Build...

:NUNITREADY
set NUNIT_BUILD=DONE

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

:RUN
REM ********************************************************
@echo Running GH tests...
REM ********************************************************

REM ********************************************************
@echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

copy Test\bin\%PROJECT_CONFIGURATION%\%TEST_ASSEMBLY% .

REM @echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUN_LOG% 2<&1
REM @echo off



REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool
msbuild %XML_TOOL_PATH%\XmlTool20.vmwcsproj /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1
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
copy %RUN_LOG% ..\
copy %BUILD_LOG% ..\
copy %GH_OUTPUT_XML% ..\

REM EXIT 0
