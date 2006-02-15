System.Web test harness
============================

This harness is a part of System.Web tests set. It's target to run web tests deployed in application 
server and compare with predefined results. (Web tests are web application)

Harness usage:

SystemWebTest.exe [-e | -t <web tests base url>] [-c <tests catalog>] [-i <ignore list>] [-o <output dir>]

-e		This will collect expected results for future test runs. Expected results should be
		collected from the same tests web application deployed in IIS on windows system.
		Parameter baseUrl must be provided.

-t		This will run all tests. Expected results must be exist at this stage.

-o		Specifies folder where expected results will be placed during collecting or where from 
		expected results will be taken during tests run. The default vaule is current folder.
		
-i		Specifies xml file with ignore filters list. Default value is almost_config.xml

-c		Specifies xml file with test cases catalog. Default value is test_catalog.xml

-na		Run without almost mechanizm.

-x		Run exluded tests. These tests have EXCLUDE="Y" attribute in test_catalog.xml file.

Examples: 

SystemWebTest.exe -t http://localhost/MainsoftWebApp
This will collect results from local web serer and store it in current folder.

SystemWebTest.exe -e http://iissite/System_Web_dll -t http://localhost:8080/MainsoftWebApp -o test123
This will collect results from iissite and store it in folder test123 then run tests on local web server.

Running with NUnit:

Build this harness with -define:NUNIT compilation constant. 
Run nunit-console.exe with /fixture:MonoTests.stand_alone.WebHarness.Harness parameter.