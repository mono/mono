@ECHO OFF 

rem =================================================
if "%GH_HOME%" == "" (set GH_HOME=c:\Program Files\Mainsoft\Visual MainWin for J2EE)
if "%JAVA_HOME%" == "" (set JAVA_HOME=%GH_HOME%\jre5) 
set JGAC_PATH=%GH_HOME%\java_refs\framework

rem =================================================
echo Hiding svn...
pushd MainsoftWebApp
FOR /R %%f IN (.svn) DO IF EXIST "%%f" ( 
ATTRIB -h "%%f" 
RENAME "%%f" _svn 
) 

rem =================================================
if "%1"=="JBoss" (
echo Building JBoss web project...
"%VS71COMNTOOLS%..\IDE\devenv.com" MainsoftWebApp.JBoss.vmwcsproj /build Debug_Java  > nul
) else (
echo Building Tomcat web project...
"%VS71COMNTOOLS%..\IDE\devenv.com" MainsoftWebApp.Tomcat.vmwcsproj /build Debug_Java  > nul
)

IF NOT ERRORLEVEL==0 (set BUILD_FAILED=TRUE)

rem =================================================
echo Restoring svn...
FOR /R %%f IN (_svn) DO IF EXIST "%%f" ( 
RENAME "%%f" .svn 
ATTRIB +h "%%~pf\.svn" 
) 
popd

rem =================================================
IF "%BUILD_FAILED%"=="TRUE" GOTO FAILURE

rem =================================================
if "%NUNIT_BUILD%" == "DONE" goto NUNITSKIP
echo Build NUnit...
pushd ..\..\..\..\nunit20\
"%VS71COMNTOOLS%..\IDE\devenv.com" nunit.java.sln /build Debug_Java > nul
popd

goto NUNITREADY
:NUNITSKIP
echo Skipping NUnit Build...
:NUNITREADY
set NUNIT_BUILD=DONE

rem =================================================
echo Build System.Web test client side...
pushd MainsoftWebTest
"%VS71COMNTOOLS%..\IDE\devenv.com" SystemWebTest.vmwcsproj /build Debug_Java_NUnit > nul
popd

rem =================================================
if "%TEST_17%" == "TRUE" goto SKIPMONO3
echo Build System.Web mono tests...
pushd ..
dos2unix System.Web.UI.HtmlControls\HtmlSelectTest.cs  > nul
dos2unix System.Web.UI.WebControls\CheckBoxListTest.cs  > nul
dos2unix System.Web.UI.WebControls\RepeatInfoTest.auto.cs  > nul
"%VS71COMNTOOLS%..\IDE\devenv.com" TestMonoWeb_jvm.vmwcsproj /build Debug_Java  > nul
popd
:SKIPMONO3

rem =================================================
copy MainsoftWebTest\almost_config.xml MainsoftWebTest\bin\almost_config.xml /Y  > nul
copy MainsoftWebTest\test_catalog.xml MainsoftWebTest\bin\test_catalog.xml /Y  > nul
copy MainsoftWebTest\App.gh.config MainsoftWebTest\bin\nunit-console.exe.config /Y  > nul
copy ..\..\..\..\nunit20\core\bin\Debug_Java\nunit.core.jar MainsoftWebTest\bin\nunit.core.jar /Y  > nul
copy ..\..\..\..\nunit20\framework\bin\Debug_Java\nunit.framework.jar MainsoftWebTest\bin\nunit.framework.jar /Y  > nul
copy ..\..\..\..\nunit20\util\bin\Debug_Java\nunit.util.jar MainsoftWebTest\bin\nunit.util.jar /Y  > nul
copy ..\..\..\..\nunit20\nunit-console\bin\Debug_Java\nunit-console.jar MainsoftWebTest\bin\nunit-console.jar /Y  > nul

rem =================================================
echo Buildinig xmltool...
pushd ..\..\..\..\tools\mono-xmltool
"%VS71COMNTOOLS%..\IDE\devenv.com" XmlTool.sln /build Debug_Java  > nul
popd
copy ..\..\..\..\tools\mono-xmltool\bin\Debug_Java\xmltool.exe MainsoftWebTest\bin\xmltool.exe  > nul
copy ..\..\..\..\tools\mono-xmltool\nunit_transform.xslt MainsoftWebTest\bin\nunit_transform.xslt  > nul

rem =================================================
set GH_CP=%JGAC_PATH%\mscorlib.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\System.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\System.Xml.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\System.Web.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\System.Data.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\System.Drawing.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\J2SE.Helpers.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\J2EE.Helpers.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\vmwutils.jar

set GH_CP=%GH_CP%;nunit.core.jar
set GH_CP=%GH_CP%;nunit.framework.jar
set GH_CP=%GH_CP%;nunit.util.jar
set GH_CP=%GH_CP%;nunit-console.jar

set ghlogfile=logfile.xml
set monologfile=mono.xml

pushd MainsoftWebTest\bin

echo Running Mainsoft tests...
"%JAVA_HOME%\bin\java.exe" -cp .;"%GH_CP%" NUnit.Console.ConsoleUi SystemWebTest.jar /xml=%ghlogfile% /fixture:MonoTests.stand_alone.WebHarness.Harness  > nul

if "%TEST_17%" == "TRUE" goto SKIPMONO
echo Running Mono tests...
"%JAVA_HOME%\bin\java.exe" -cp .;"%GH_CP%" NUnit.Console.ConsoleUi TestMonoWeb_jvm.jar /xml=%monologfile% /exclude:NotWorking,ValueAdd,InetAccess /fixture:MonoTests.System.Web  > nul
:SKIPMONO

echo Finished...
xmltool.exe --transform nunit_transform.xslt %ghlogfile%

if "%TEST_17%" == "TRUE" goto SKIPMONO2
xmltool.exe --transform nunit_transform.xslt %monologfile%
:SKIPMONO2

popd

goto :END
:FAILURE
popd
echo Failed during build...
set BUILD_FAILED=
:END


