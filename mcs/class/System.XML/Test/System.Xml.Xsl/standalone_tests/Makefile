RUNTIME = mono
MCS_RUNTIME =
MCS = mcs
TESTS = testsuite/TESTS/Xalan_Conformance_Tests/whitespace/whitespace35.xsl
RESULTS = testsuite/TESTS/Xalan_Conformance_Tests/whitespace/whitespace35.out

xslttest.exe : xslttest.cs $(TESTS) $(RESULTS)
	$(MCS_RUNTIME) $(MCS) xslttest.cs

$(TESTS) : xslt-testsuite-01.zip
	unzip -n xslt-testsuite-01.zip
	mv testsuite/TESTS/Xalan_Conformance_Tests/catalog.xml testsuite/TESTS/Xalan_Conformance_Tests/catalog-org.xml
	cp xalan-correct-catalog.xml testsuite/TESTS/Xalan_Conformance_Tests/catalog.xml

$(RESULTS) :
	tar zxvf testresults.tar.gz

xslt-testsuite-01.zip :
	wget http://www.oasis-open.org/committees/download.php/1937/xslt-testsuite-01.zip

run-test :
	$(RUNTIME) xslttest.exe --report:TestResult.xml --xml --details --outall

# Be careful to use this!
# clean :
#	rm -rf testsuite
