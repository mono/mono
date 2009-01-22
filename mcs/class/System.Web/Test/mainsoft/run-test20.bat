@ECHO OFF 

rem =================================================
if "%GH_HOME%" == "" (set GH_HOME=c:\Program Files\Mainsoft\Visual MainWin for J2EE V2)
if "%VMW_HOME%" == "" (set VMW_HOME=%GH_HOME%) 
if "%JAVA_HOME%" == "" (set JAVA_HOME=%GH_HOME%\jre) 
set JGAC_PATH=%GH_HOME%\java_refs\framework

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

set OUTPUT_FILE_PREFIX=System_Web_Services
set RUNNING_FIXTURE=System.Web.Services

set COMMON_PREFIX=%cd%\%TIMESTAMP%_%OUTPUT_FILE_PREFIX%.GH_%GH_VERSION%.1.%USERNAME%
set GH_OUTPUT_XML=%COMMON_PREFIX%.xml
set BUILD_LOG=%COMMON_PREFIX%.build.log
set RUN_LOG=%COMMON_PREFIX%.run.log

rem =================================================
pushd MainsoftWebApp
echo Building Tomcat web project...
msbuild MainsoftWebApp20.Tomcat.csproj /t:deploy /p:Configuration=Debug_Java >>%BUILD_LOG% 2<&1
popd

IF NOT ERRORLEVEL==0 GOTO FAILURE

rem =================================================
if "%NUNIT_BUILD%" == "DONE" goto NUNITSKIP
echo Build NUnit...
pushd ..\..\..\..\nunit20\
msbuild nunit20.java.sln /t:build /p:Configuration=Debug_Java20 >>%BUILD_LOG% 2<&1
popd

goto NUNITREADY
:NUNITSKIP
echo Skipping NUnit Build...
:NUNITREADY
set NUNIT_BUILD=DONE

rem =================================================
echo Build System.Web test client side...
pushd MainsoftWebTest
msbuild SystemWebTest20.J2EE.csproj /t:build /p:Configuration=Debug_Java_Nunit >>%BUILD_LOG% 2<&1
popd


rem =================================================
copy MainsoftWebTest\almost_config.xml MainsoftWebTest\bin\almost_config.xml /Y  
copy MainsoftWebTest\test_catalog.xml MainsoftWebTest\bin\test_catalog.xml /Y  
copy MainsoftWebTest\App.gh20.config MainsoftWebTest\bin\nunit-console.exe.config /Y  
copy ..\..\..\..\nunit20\core\bin\Debug_Java\nunit.core.jar MainsoftWebTest\bin\nunit.core.jar /Y  
copy ..\..\..\..\nunit20\framework\bin\Debug_Java\nunit.framework.jar MainsoftWebTest\bin\nunit.framework.jar /Y  
copy ..\..\..\..\nunit20\util\bin\Debug_Java\nunit.util.jar MainsoftWebTest\bin\nunit.util.jar /Y 
copy ..\..\..\..\nunit20\nunit-console\bin\Debug_Java\nunit-console.jar MainsoftWebTest\bin\nunit-console.jar /Y 

rem =================================================
echo Buildinig xmltool...
pushd ..\..\..\..\tools\mono-xmltool
msbuild XmlTool20.csproj /p:Configuration=Debug_Java20 >>%BUILD_LOG% 2<&1
popd
copy ..\..\..\..\tools\mono-xmltool\bin\Debug_Java\xmltool.exe MainsoftWebTest\bin\xmltool.exe 
copy ..\..\..\..\tools\mono-xmltool\nunit_transform.xslt MainsoftWebTest\bin\nunit_transform.xslt 

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

pushd MainsoftWebTest\bin

echo Running Mainsoft tests...
"%JAVA_HOME%\bin\java.exe" -cp .;"%GH_CP%" NUnit.Console.ConsoleUi SystemWebTest.jar /xml=%GH_OUTPUT_XML% /fixture:MonoTests.stand_alone.WebHarness.Harness >>%RUN_LOG% 2<&1

echo Finished...
xmltool.exe --transform nunit_transform.xslt %GH_OUTPUT_XML%

popd

goto :END
:FAILURE
popd
echo Failed during build...
set BUILD_FAILED=
:END


