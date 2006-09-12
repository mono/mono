@echo off
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM output files name prefix (mandratory) : prefix for naming output xml files
REM secure mode (mandratory): secure or non-secure mode indicator
REM test fixture name (optional) : if you want to run some particular test fixture
REM example run-tests build GhTests Test.Sys.Drawing  
REM will cause to build (and not rebuild) test solutions,
REM running Test.Sys.Drawing fixture 
REM with output files named GhTests.Net.xml and GhTests.GH.xml
REM ********************************************************


IF "%1"=="" GOTO USAGE

IF "%JAVA_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION

IF "%GHROOT%"=="" GOTO ENVIRONMENT_EXCEPTION

REM ********************************************************
REM Set parameters
REM ********************************************************

set BUILD_OPTION=%1
set SECURE_MODE=%2
set OUTPUT_FILE_PREFIX=%3
set RUNNING_FIXTURE=%4
set TEST_SOLUTION=Test\System.DirectoryServices.Test20.sln
set TEST_ASSEMBLY=System.DirectoryServices.Test20.jar
set PROJECT_CONFIGURATION=Debug_Java20

set DATEL=%date:~4,2%_%date:~7,2%_%date:~10,4%
set TIMEL=%time:~0,2%_%time:~3,2%
set TIMESTAMP=%DATEL%_%TIMEL%


REM ********************************************************
REM @echo Set environment
REM ********************************************************

set JGAC_PATH=%GHROOT%\jgac\vmw4j2ee_110\

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar;%JGAC_PATH%System.jar;%JGAC_PATH%System.Xml.jar;%JGAC_PATH%System.DirectoryServices.jar;%JGAC_PATH%Novell.Directory.Ldap.jar;%JGAC_PATH%J2SE.Helpers.jar
set NUNIT_OPTIONS=/exclude=NotWorking

set GH_OUTPUT_XML=%OUTPUT_FILE_PREFIX%.GH.%SECURE_MODE%.%TIMESTAMP%.xml
set BUILD_LOG=%OUTPUT_FILE_PREFIX%.GH.%SECURE_MODE%.%RUNNING_FIXTURE%_build.log.%TIMESTAMP%.txt
set RUN_LOG=%OUTPUT_FILE_PREFIX%.GH.%SECURE_MODE%.%RUNNING_FIXTURE%_run.log.%TIMESTAMP%.txt

set NUNIT_PATH=%BACK_TO_ROOT_DIR%..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.framework.jar;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.util.jar;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.core.jar;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit-console.jar;.
set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"


REM ********************************************************
REM @echo Building GH solution...
REM ********************************************************

rem devenv Test\System.DirectoryServices.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild %TEST_SOLUTION% /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
REM @echo Building NUnit solution...
REM ********************************************************

if "%NUNIT_BUILD%" == "DONE" goto NUNITSKIP

rem devenv ..\..\nunit20\nunit.java.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild ..\..\nunit20\nunit20.java.sln /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

goto NUNITREADY

:NUNITSKIP
echo Skipping NUnit Build...

:NUNITREADY
set NUNIT_BUILD=DONE

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
REM @echo Running GH tests...
REM ********************************************************

REM ********************************************************
REM @echo Running fixture "%RUNNING_FIXTURE%"
REM ********************************************************

copy Test\bin\Debug_Java20\%TEST_ASSEMBLY% .

IF "%SECURE_MODE%" NEQ "secure" (
	copy App.config nunit-console.exe.config 
	set JVM_OPTIONS=-Xmx1024M
) ELSE (
	copy Secure.config nunit-console.exe.config 
	set JVM_OPTIONS=-Djava.security.krb5.conf=Test\krb5.conf.example -Djava.security.auth.login.config=Test\java.login.sun.config -Xmx1024M
)

REM @echo on
"%JAVA_HOME%\bin\java" %JVM_OPTIONS%  -cp %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUN_LOG% 2<&1
REM @echo off


REM ********************************************************
REM @echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool
rem devenv %XML_TOOL_PATH%\XmlTool.sln /%BUILD_OPTION% Debug_Java >>%RUNNING_FIXTURE%_build.log.txt 2<&1
msbuild %XML_TOOL_PATH%\XmlTool20.vmwcsproj /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

copy %XML_TOOL_PATH%\bin\%PROJECT_CONFIGURATION%\xmltool.exe .
copy %XML_TOOL_PATH%\nunit_transform.xslt .

REM ********************************************************
REM @echo Analyze and print results
REM ********************************************************
REM @echo on
xmltool.exe --transform nunit_transform.xslt %GH_OUTPUT_XML%
REM @echo off

copy %RUN_LOG% ..\
copy %BUILD_LOG% ..\
copy %GH_OUTPUT_XML% ..\


:FINALLY
GOTO END

:ENVIRONMENT_EXCEPTION
@echo This test requires environment variables JAVA_HOME and GHROOT to be defined
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
@echo Parameters: "[build|rebuild] <output_file_name_prefix> <test_fixture>"
GOTO END

:END
REM EXIT 0
