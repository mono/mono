@ECHO OFF 

rem =================================================
if "%GH_HOME%" == "" (set GH_HOME=c:\Program Files\Mainsoft\Visual MainWin for J2EE)
if "%JAVA_HOME%" == "" (set JAVA_HOME=%GH_HOME%\jre5) 
set JGAC_PATH=%GH_HOME%\jgac\vmw4j2ee_110

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
"%VS71COMNTOOLS%..\IDE\devenv.com" MainsoftWebApp.JBoss.vmwcsproj /build Debug_Java
) else (
echo Building Tomcat web project...
"%VS71COMNTOOLS%..\IDE\devenv.com" MainsoftWebApp.Tomcat.vmwcsproj /build Debug_Java
)


rem =================================================
echo Restoring svn...
FOR /R %%f IN (_svn) DO IF EXIST "%%f" ( 
RENAME "%%f" .svn 
ATTRIB +h "%%~pf\.svn" 
) 
popd

rem =================================================
echo Build NUnit...
pushd ..\..\..\..\nunit20\
"%VS71COMNTOOLS%..\IDE\devenv.com" nunit.java.sln /build Debug_Java
popd

rem =================================================
echo Build System.Web test client side...
pushd MainsoftWebTest
"%VS71COMNTOOLS%..\IDE\devenv.com" SystemWebTest.vmwcsproj /build Debug_Java_NUnit
popd

rem =================================================
echo Build System.Web mono tests...
pushd ..
dos2unix System.Web.UI.HtmlControls\HtmlSelectTest.cs
dos2unix System.Web.UI.WebControls\CheckBoxListTest.cs
dos2unix System.Web.UI.WebControls\RepeatInfoTest.auto.cs
"%VS71COMNTOOLS%..\IDE\devenv.com" TestMonoWeb_jvm.vmwcsproj /build Debug_Java
popd

rem =================================================
copy MainsoftWebTest\almost_config.xml MainsoftWebTest\bin\almost_config.xml /Y
copy MainsoftWebTest\test_catalog.xml MainsoftWebTest\bin\test_catalog.xml /Y
copy MainsoftWebTest\App.gh.config MainsoftWebTest\bin\nunit-console.exe.config /Y
copy ..\..\..\..\nunit20\core\bin\Debug_Java\nunit.core.jar MainsoftWebTest\bin\nunit.core.jar /Y
copy ..\..\..\..\nunit20\framework\bin\Debug_Java\nunit.framework.jar MainsoftWebTest\bin\nunit.framework.jar /Y
copy ..\..\..\..\nunit20\util\bin\Debug_Java\nunit.util.jar MainsoftWebTest\bin\nunit.util.jar /Y
copy ..\..\..\..\nunit20\nunit-console\bin\Debug_Java\nunit-console.jar MainsoftWebTest\bin\nunit-console.jar /Y

rem =================================================
echo Buildinig xmltool...
pushd ..\..\..\..\tools\mono-xmltool
"%VS71COMNTOOLS%..\IDE\devenv.com" XmlTool.sln /build Debug_Java
popd
copy ..\..\..\..\tools\mono-xmltool\bin\Debug_Java\xmltool.exe MainsoftWebTest\bin\xmltool.exe
copy ..\..\..\..\tools\mono-xmltool\nunit_transform.xslt MainsoftWebTest\bin\nunit_transform.xslt

rem =================================================
echo Running...

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

"%JAVA_HOME%\bin\java.exe" -cp .;"%GH_CP%" NUnit.Console.ConsoleUi SystemWebTest.jar /xml=%ghlogfile% /fixture:MonoTests.stand_alone.WebHarness.Harness
"%JAVA_HOME%\bin\java.exe" -cp .;"%GH_CP%" NUnit.Console.ConsoleUi TestMonoWeb_jvm.jar /xml=%monologfile% /exclude:NotWorking,ValueAdd,InetAccess /fixture:MonoTests.System.Web

echo Finished...
xmltool.exe --transform nunit_transform.xslt %ghlogfile%
xmltool.exe --transform nunit_transform.xslt %monologfile%

popd

