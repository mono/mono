@echo off
REM ********************************************************
REM This batch file receives the follwing parameters:
REM build/rebuild (optional): should the solution file be rebuilded 
REM                             or just builded before test run (default is rebuild)
REM example run-tests build 
REM will cause to build (and not rebuild) test solutions,
REM ********************************************************

IF "%1"=="" GOTO USAGE

IF "%VMW_HOME%"=="" GOTO ENVIRONMENT_EXCEPTION



IF "%1"=="" (
	set BUILD_OPTION=rebuild
) ELSE (
	set BUILD_OPTION=%1
)

REM ********************************************************
REM Set parameters
REM ********************************************************

set BUILD_OPTION=%1
set OUTPUT_FILE_PREFIX=System.XML.XSLT
set RUNNING_FIXTURE=MonoTests.oasis_xslt.SuiteBuilder
set TEST_SOLUTION=xslt20.J2EE.sln
set TEST_ASSEMBLY=xslt.jar
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
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%J2SE.Helpers.jar
set NUNIT_OPTIONS=/include=Clean

set GH_OUTPUT_XML=%TIMESTAMP%.%OUTPUT_FILE_PREFIX%.GH.xml
set BUILD_LOG=%TIMESTAMP%.%OUTPUT_FILE_PREFIX%.GH.%RUNNING_FIXTURE%.build.log
set RUN_LOG=%TIMESTAMP%.%OUTPUT_FILE_PREFIX%.GH.%RUNNING_FIXTURE%.run.log

set NUNIT_PATH=..\..\..\..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.framework.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.util.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.core.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit-console.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;.
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%TEST_ASSEMBLY%

set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%"
set XSLT_DIR=Test\System.Xml.XSL\standalone_tests\

pushd %XSLT_DIR%
IF "%BUILD_OPTION%"=="nobuild" GOTO RUN

REM ********************************************************
@echo Building GH solution...
REM ********************************************************

msbuild %TEST_SOLUTION% /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1

IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Building test catalog...
REM ********************************************************
del xslt-testsuite-03.ZIP
wget http://www.oasis-open.org/committees/download.php/9584/xslt-testsuite-03.ZIP
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION
unzip -un xslt-testsuite-03.ZIP
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION
del xslt-reference-results.tar.gz
wget http://svn.myrealbox.com/source/trunk/release/test-ext/xslt-standalone/xslt-reference-results.tar.gz
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION
tar -xzf xslt-reference-results.tar.gz
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION
sed -f catalog.sed testsuite/TESTS/catalog.xml > testsuite/TESTS/catalog-fixed.xml
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION
patch -p0 -i catalog-fixed.diff
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

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

REM @echo on
"%JAVA_HOME%\bin\java" -Xmx1024M -cp %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% /fixture=%RUNNING_FIXTURE%  %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUN_LOG% 2<&1
REM @echo off

popd

copy %XSLT_DIR%\%GH_OUTPUT_XML% .
copy %XSLT_DIR%\%RUN_LOG% .

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
@echo This test requires environment variables JAVA_HOME and GH_HOME to be defined
GOTO END

:BUILD_EXCEPTION
popd
@echo Error in building solutions. See %BUILD_LOG% for details...
GOTO END

:RUN_EXCEPTION

@echo Error in running fixture %RUNNING_FIXTURE%. See %RUN_LOG% for details...
GOTO END

:USAGE
@echo Parameters: "[build|rebuild] <output_file_name_prefix> <test_fixture> <relative_Working_directory> <back_path (..\..\.....) >"
GOTO END

:END
copy %RUN_LOG% ..\
copy %BUILD_LOG% ..\
copy %GH_OUTPUT_XML% ..\

REM EXIT 0
