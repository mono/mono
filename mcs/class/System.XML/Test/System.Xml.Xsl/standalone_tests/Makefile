#
# IMPORTANT: this test runner is right now incompatible with existing 
# test runner (xslttest.exe).
#
# "make" prepares automated download and build for both existing xslttest
# and MainSoft tests.
#
# "make run-test" executes both existing xslttest and MainSoft's new test
# and reports the total diff numbers:
#
#	- For xslttest, it is "TestResult.xml" and
#	- For MainSoft test, it is "diffreport.txt"
#

RUNTIME = mono
MCS_RUNTIME =
MCS = mcs
TESTS = testsuite/TESTS/Xalan_Conformance_Tests/whitespace/whitespace35.xsl
MSTEST_RESULTS = testsuite/TESTS/ExpectedResults
TEST_ARCHIVE = xslt-testsuite-03.ZIP
XSLTTEST_RESULTS_ARCHIVE = mono-xslttest-ExpectedResults.zip
MSTEST_RESULTS_ARCHIVE = mono-xslttest-ExpectedResults.zip

xslttest.exe : xslttest.cs $(TESTS) $(XSLTTEST_RESULTS)

alltest.exe : alltest.cs $(TESTS) $(MSTEST_RESULTS)
	$(MCS_RUNTIME) $(MCS) alltest.cs

$(TESTS) : $(TEST_ARCHIVE) simplify.xsl
	unzip -n $(TEST_ARCHIVE)
	xsltproc simplify.xsl testsuite/TESTS/catalog.xml > testsuite/TESTS/catalog-out.xml

$(XSLTTEST_RESULTS) : $(XSLTTEST_RESULTS_ARCHIVE)
	unzip -n -d $(XSLTTEST_RESULTS_ARCHIVE)

$(MSTEST_RESULTS) : $(MSTEST_RESULTS_ARCHIVE)
	unzip -n -d testsuite/TESTS $(MSTEST_RESULTS_ARCHIVE)

$(TEST_ARCHIVE) :
	wget http://www.oasis-open.org/committees/download.php/9584/$(TEST_ARCHIVE)

$(XSLTTEST_RESULTS_ARCHIVE) :
	wget http://monkey.workarea.jp/mono/xml/$(XSLTTEST_RESULTS_ARCHIVE)

$(MSTEST_RESULTS_ARCHIVE) :
	wget http://monkey.workarea.jp/mono/xml/$(MSTEST_RESULTS_ARCHIVE)

run-test : run-test-xslt run-test-ms

run-test-xslt :
	$(RUNTIME) xslttest.exe --report:TestResult.xml --xml --details --outall

run-test-ms :
	$(RUNTIME) alltest.exe Results
	diff -r -q testsuite/TESTS/ExpectedResults testsuite/TESTS/Results > diffreport.txt

clean : clean-original clean-ms

clean-original :
	find testsuite/TESTS -name *.out > output.lst
	perl cleanup-output.pl

clean-ms :
	rm -rf testsuite/TESTS/Reports
	rm diffreport.txt alltest.exe

# Be careful to use it!
distclean :
	rm -rf testsuite
	rm diffreport.txt alltest.exe $(TEST_ARCHIVE) $(XSLTTEST_RESULTS_ARCHIVE) $(MSTEST_RESULTS_ARCHIVE)

create-reference-output : clean-original xslttest.exe
	./xslttest.exe --generate
	find testsuite/TESTS -name *.out > output.lst
	zip xslttest-reference-output.zip -@ < output.lst

