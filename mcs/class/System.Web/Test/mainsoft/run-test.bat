@ECHO OFF 

rem =================================================
set JAVA_HOME=c:\j2sdk1.4.2_09
set JGAC_PATH=c:\Program Files\Mainsoft\Visual MainWin for J2EE\jgac\vmw4j2ee_110

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
set GH_CP=%GH_CP%;%JGAC_PATH%\J2SE.Helpers.jar
set GH_CP=%GH_CP%;%JGAC_PATH%\vmwutils.jar

set GH_CP=%GH_CP%;nunit.core.jar
set GH_CP=%GH_CP%;nunit.framework.jar
set GH_CP=%GH_CP%;nunit.util.jar
set GH_CP=%GH_CP%;nunit-console.jar

set logfile=logfile.xml

pushd MainsoftWebTest\bin
%JAVA_HOME%\bin\java.exe -cp .;"%GH_CP%" NUnit.Console.ConsoleUi SystemWebTest.jar /xml=%logfile% /fixture:MonoTests.stand_alone.WebHarness.Harness

echo Finished...
xmltool.exe --transform nunit_transform.xslt %logfile%

popd

