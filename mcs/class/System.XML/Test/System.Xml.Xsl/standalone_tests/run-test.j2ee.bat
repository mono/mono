set JGAC_PATH=c:\Program Files\Mainsoft\Visual MainWin for J2EE\java_refs\framework\
java -cp "%JGAC_PATH%mscorlib.jar;%JGAC_PATH%System.jar;%JGAC_PATH%System.Xml.jar;%JGAC_PATH%nunit.core.jar;%JGAC_PATH%nunit.framework.jar;%JGAC_PATH%nunit.util.jar;%JGAC_PATH%J2SE.Helpers.jar;xslt.jar;nunit-console.jar" NUnit.Console.ConsoleUi xslt.jar /fixture:MonoTests.oasis_xslt.SuiteBuilder /xml=TestResult.xml /include=KnownFailures

