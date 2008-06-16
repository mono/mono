@echo off
setlocal

:loop
if "%1" == "" goto break
if "%1" == "skip_selenium" (
	set SKIP_SELENIUM="True"
)
if "%1" == "skip_app" (
	set SKIP_APP="True"
)
if "%1" == "skip_tests" (
	set SKIP_TESTS="True"
)
shift /1
goto loop
:break

rem SETUP ENVIRONMENT FOR RUNNING SELENIUM TESTS
rem ============================================
if "%VMW_HOME%" == "" set VMW_HOME=C:\Program Files\Mainsoft for Java EE

if NOT "%SELENIUM_HOME%" == "" goto after_set_SELENIUM_HOME
set SELENIUM_HOME=%~dp0
set SELENIUM_HOME=%SELENIUM_HOME:class\System.Web.Extensions=selenium%
:after_set_SELENIUM_HOME
echo SELENIUM_HOME=%SELENIUM_HOME%

set Browser=C:\Program Files\Internet Explorer\iexplore.exe
set HTTPServer=http://localhost:8080
set SeleniumURL=%HTTPServer%/Selenium

rem =================================================
set startDate=%date%
set startTime=%time%
set sdy=%startDate:~10%
set /a sdm=1%startDate:~4,2% - 100
set /a sdd=1%startDate:~7,2% - 100
set /a sth=%startTime:~0,2%
set /a stm=1%startTime:~3,2% - 100
set /a sts=1%startTime:~6,2% - 100
set TIMESTAMP=%sdy%_%sdm%_%sdd%_%sth%_%stm%

set ResultsURL=/PostResults
set ResultsDir=FuncTests%TIMESTAMP%
set OUTPUT_FILE_PREFIX=SystemWebExtensionsSelenium

set COMMON_PREFIX=%cd%\%TIMESTAMP%_%OUTPUT_FILE_PREFIX%.GH_%GH_VERSION%.1.%USERNAME%
set SELENIUM_OUTPUT_XML=%COMMON_PREFIX%.xml
set BUILD_LOG=%COMMON_PREFIX%.build.log
set RUN_LOG=%COMMON_PREFIX%.run.log

rem DEPLOY SELENIUM WITH TESTS TO SERVER
rem ====================================
if DEFINED SKIP_SELENIUM goto after_selenium
echo Deploying Selenium
call %SELENIUM_HOME%\DeploySelenium.cmd "Tomcat" "%SELENIUM_HOME%\TomcatDeploy.cmd" "http://admin:admin@localhost:8080" >>%BUILD_LOG% 2<&1
:after_selenium

rem BUILD APPLICATION UNDER TEST
rem ============================================
if DEFINED SKIP_APP goto after_app
pushd Test\AUT
echo Building %cd%\SystemWebExtensionsAUT.JavaEE.csproj
del /F /Q bin_Java\deployedFiles bin_Java\outputFiles.list
msbuild SystemWebExtensionsAUT.JavaEE.csproj /t:Rebuild /t:Deploy /p:Configuration=Debug_Java >>%BUILD_LOG% 2<&1
IF %ERRORLEVEL% NEQ 0 GOTO BUILD_EXCEPTION
popd
:after_app

if DEFINED SKIP_TESTS goto after_tests

echo Running Functional Test Suites

mkdir %ResultsDir%
type %SELENIUM_HOME%\SeleniumTestResultsHead.txt >%SELENIUM_OUTPUT_XML%

wget -O .\nul "%HTTPServer%%ResultsURL%/Default.ashx" >>%RUN_LOG% 2<&1
wget -O "%ResultsDir%\selenium-test.css" "%HTTPServer%%ResultsURL%/selenium-test.css" >>%RUN_LOG% 2<&1

rem RUN THE TEST SUITES ONE AFTER THE OTHER
rem ============================================

call :executeTestSuite /SystemWebExtensionsAUT/Selenium/System.Web.UI/UpdatePanel/UpdatePanelTestSuite.html
call :executeTestSuite /SystemWebExtensionsAUT/Selenium/Sys.WebForms/PageRequestManager/PageRequestManagerTestSuite.html
call :executeTestSuite /SystemWebExtensionsAUT/Selenium/QuickStarts/QuickStartTestSuite.html


rem ADD MORE TEST SUITES ABOVE THIS LINE
rem ====================================

type %SELENIUM_HOME%\SeleniumTestResultsTail.txt >>%SELENIUM_OUTPUT_XML%

:after_tests
goto afterExecuteTestSuite

rem INTERNAL SCRIPT FUNCTION TO RUN SPECIFIC TEST SUITE
rem ===================================================
:executeTestSuite

set TestSuiteRelativePath=%1
set SuiteName=%~n1
set ResultsAsXML=%ResultsDir%\%SuiteName%Results.xml
set ResultsAsHtml=%ResultsDir%\%SuiteName%Results.html

echo Test suite: %SuiteName%
echo Test suite: %SuiteName% >>%RUN_LOG% 2<&1
"%Browser%" "%SeleniumURL%/core/TestRunner.html?test=%TestSuiteRelativePath%&auto=true&close=on&multiWindow=off&resultsUrl=%ResultsURL%/Default.ashx"

if NOT %ResultsAsXML%=="" (
	wget -O "%ResultsAsXML%" "%HTTPServer%%ResultsURL%/GetLastResults.ashx" >>%RUN_LOG% 2<&1
	type "%ResultsAsXML%" >>%SELENIUM_OUTPUT_XML%
)

if NOT %ResultsAsHtml%=="" (
	wget -O "%ResultsAsHtml%" "%HTTPServer%%ResultsURL%/GetLastResults.ashx?Html" >>%RUN_LOG% 2<&1
)

exit /B

goto END
:BUILD_EXCEPTION
@echo Error in building solutions. See %BUILD_LOG% for details...
REM EXIT 1
GOTO END

:afterExecuteTestSuite
:END
endlocal

