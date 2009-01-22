@echo off
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

set OUTPUT_FILE_PREFIX=System_Web
set RUNNING_FIXTURE=System.Web
set TEST_SOLUTION=Test\TestMonoWeb.J2EE20.sln
set DEPLOY_PROJECT=Test\mainsoft\MainsoftWebApp20\MainsoftWebApp20.csproj
set TEST_ASSEMBLY=TestMonoWeb.jar
set PROJECT_CONFIGURATION=Debug_Java20


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
set TOMCAT_PATH=%VMW_HOME%\jakarta-tomcat\common\lib\
set JAVA_HOME=%VMW_HOME%\jre

set RUNTIME_CLASSPATH=%JGAC_PATH%mscorlib.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Xml.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Web.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Runtime.Serialization.Formatters.Soap.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%Mainsoft.Web.jar
rem set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%System.Drawing.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%JGAC_PATH%J2SE.Helpers.jar

set FACES_PATH=%JGAC_PATH%myfaces\
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%commons-beanutils-1.7.0.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%commons-codec-1.3.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%commons-collections-3.1.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%commons-digester-1.6.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%commons-el-1.0.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%commons-lang-2.1.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%commons-logging-1.0.4.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%jstl-1.1.0.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%myfaces-api-1.1.5.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%FACES_PATH%myfaces-impl-1.1.5.jar

set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;%VMW_HOME%\java_refs\jdbc\derby.jar

rem ./Test/mainsoft/MainsoftWebTest/bin/Debug_Java20/HtmlCompare.jar

set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;Test\mainsoft\NunitWeb\NunitWeb\bin\Debug_Java20\NunitWeb.jar
set RUNTIME_CLASSPATH=%RUNTIME_CLASSPATH%;Test\mainsoft\MainsoftWebTest\bin\Debug_Java20\HtmlCompare.jar

set TOMCAT_CLASSPATH=%TOMCAT_PATH%servlet-api.jar
set NUNIT_OPTIONS=/exclude=NotWorking

if "%GH_VERSION%"=="" (
	set GH_VERSION=0_0_0_0
)

set COMMON_PREFIX=%TIMESTAMP%_%OUTPUT_FILE_PREFIX%.GH_%GH_VERSION%.1.%USERNAME%
set GH_OUTPUT_XML=%COMMON_PREFIX%.xml
set BUILD_LOG=%COMMON_PREFIX%.build.log
set RUN_LOG=%COMMON_PREFIX%.run.log

set NUNIT_PATH=..\..\nunit20\
set NUNIT_CLASSPATH=%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.framework.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.util.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit.core.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%NUNIT_PATH%nunit-console\bin\%PROJECT_CONFIGURATION%\nunit-console.jar
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;.
set NUNIT_CLASSPATH=%NUNIT_CLASSPATH%;%TEST_ASSEMBLY%

set CLASSPATH="%RUNTIME_CLASSPATH%;%NUNIT_CLASSPATH%;%TOMCAT_CLASSPATH%"

IF "%BUILD_OPTION%"=="nobuild" GOTO RUN

REM ********************************************************
@echo Building GH solution...
REM ********************************************************
xcopy /y Test\mainsoft\NunitWeb\NunitWeb\Resources\*.as* Test\mainsoft\MainsoftWebApp20\
xcopy /y Test\mainsoft\NunitWeb\NunitWeb\Resources\*.master Test\mainsoft\MainsoftWebApp20\
xcopy /y Test\mainsoft\NunitWebResources\*.* Test\mainsoft\MainsoftWebApp20\
pushd Test\mainsoft\MainsoftWebApp20\
xcopy /y WizardTest.skin App_Themes\Theme1\
xcopy /y Theme1.skin App_Themes\Theme1\
xcopy /y Theme2.skin App_Themes\Theme2\
xcopy /y ResolveUrl.ascx Folder\
mkdir XXX
for %%i in (*.as*) DO sed "s/CodeFile=/CodeBehind=/" %%i > XXX\%%i
move /Y XXX\* .
rmdir XXX
popd

msbuild %TEST_SOLUTION% /t:%BUILD_OPTION% /p:Configuration=%PROJECT_CONFIGURATION% >>%BUILD_LOG% 2<&1
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION

REM ********************************************************
@echo Deploying GH solution to tomcat...
REM ********************************************************
IF NOT DEFINED VMW_BUILDER GOTO MSBUILD_DEPLOY
set CATALINA_HOME=%VMW_HOME%\jakarta-tomcat
call "%VMW_HOME%\jakarta-tomcat\bin\shutdown.bat"
echo Waiting 5 sec for tomcat to stop....
@ping 127.0.0.1 -n 5 -w 1000 > nul
del /q "%VMW_HOME%\jakarta-tomcat\webapps\MainsoftWebApp20.war"
rmdir /s /q "%VMW_HOME%\jakarta-tomcat\webapps\MainsoftWebApp20"
xcopy /Y Test\mainsoft\MainsoftWebApp20\bin\%PROJECT_CONFIGURATION%\MainsoftWebApp20.war "%VMW_HOME%\jakarta-tomcat\webapps"
call "%VMW_HOME%\jakarta-tomcat\bin\startup.bat"
echo Waiting 5 sec for tomcat to start....
@ping 127.0.0.1 -n 5 -w 1000 > nul
GOTO AFTER_DEPLOY
:MSBUILD_DEPLOY
echo msbuild %DEPLOY_PROJECT% /t:Deploy /p:Configuration=%PROJECT_CONFIGURATION% /p:Platform=AnyCPU >>%BUILD_LOG% 
msbuild %DEPLOY_PROJECT% /t:Deploy /p:Configuration=%PROJECT_CONFIGURATION% /p:Platform=AnyCPU >>%BUILD_LOG% 2<&1
:AFTER_DEPLOY

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
"%JAVA_HOME%\bin\java" -Xmx1024M -XX:MaxPermSize=128m -cp %CLASSPATH% NUnit.Console.ConsoleUi %TEST_ASSEMBLY% %NUNIT_OPTIONS% /xml=%GH_OUTPUT_XML% >>%RUN_LOG% 2<&1
REM @echo off

IF NOT DEFINED VMW_BUILDER GOTO DONT_SHUTDOWN_TOMCAT
set CATALINA_HOME=%VMW_HOME%\jakarta-tomcat
call "%VMW_HOME%\jakarta-tomcat\bin\shutdown.bat"
:DONT_SHUTDOWN_TOMCAT

REM ********************************************************
@echo Build XmlTool
REM ********************************************************
set XML_TOOL_PATH=..\..\tools\mono-xmltool

if "%XMLTOOL_BUILD%" == "DONE" goto XMLTOOLSKIP

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
copy %RUN_LOG% ..\
copy %BUILD_LOG% ..\
copy %GH_OUTPUT_XML% ..\

REM EXIT 0
