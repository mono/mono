==============================
	
	NIST_DOM Readme

Author: Mizrahi Rafael 
rafim@mainsoft.com
==============================


==============================
About the Test Suite:
==============================
NIST DOM is XML DOM Level 1 Test Suite,
Mainsoft have converted part of the NIST DOM 
Test Suite 
from ECMAScript (Java Script) into 
C# .Net System.XML Conformance Test Suite.

NIST - National Institute of Standards and Technology.
www.nist.gov


==============================
Architecture of the test suite:
==============================
The test suite is devided to unit tests which test every part of the DOM specifications.
We ported to System.XML only the parts that relates to the DOM xml parser, and not the HTML DOM tests.

==============================
Files in use and their purpose:
==============================
The ECMAScript files that has been ported into C# remain,
to give a reference to the old Test Suite.
files directory contains xml and html files of the TestSuite.
These files are specific for the tests and cannot be replaced with other files.
The unit tests in the TestSuite tight to the input files.

