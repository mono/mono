RUNTIME =
MCS = mcs
TESTS = testsuite/TESTS/Xalan_Conformance_Tests/whitespace/whitespace35.xsl

xslttest.exe : xslttest.cs $(TESTS)
	$(RUNTIME) $(MCS) xslttest.cs

$(TESTS) : xslt-testsuite-01.zip
	unzip -n xslt-testsuite-01.zip
	mv testsuite/TESTS/Xalan_Conformance_Tests/catalog.xml testsuite/TESTS/Xalan_Conformance_Tests/catalog-org.xml
	cp xalan-correct-catalog.xml testsuite/TESTS/Xalan_Conformance_Tests/catalog.xml

xslt-testsuite-01.zip :
	wget http://www.oasis-open.org/committees/download.php/1937/xslt-testsuite-01.zip

