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
CSCOMPILE = mcs
TESTS = testsuite/TESTS/Xalan_Conformance_Tests/whitespace/whitespace35.xsl
TARGET_RESULTS = Results
MSTEST_RESULTS = testsuite/TESTS/ExpectedResults
TEST_ARCHIVE = xslt-testsuite-03.ZIP
XSLTTEST_RESULTS_ARCHIVE = xslttest-reference-output.zip
MSTEST_RESULTS_ARCHIVE = mono-xslttest-ExpectedResults-20050214.zip

all : xslttest.exe alltest.exe

xslttest.exe : xslttest.cs $(TESTS) $(XSLTTEST_RESULTS)
	$(CSCOMPILE) xslttest.cs

xmlnorm.exe : XmlNormalizer.cs
	$(CSCOMPILE) XmlNormalizer.cs -out:xmlnorm.exe

alltest.exe : alltest.cs $(TESTS) $(MSTEST_RESULTS)
	$(CSCOMPILE) alltest.cs

# WARNING: It removes catalog.xml. If you want to keep catalog.xml, comment
# "rm" and "patch" lines out.
$(TESTS) : $(TEST_ARCHIVE) simplify.xsl
	rm -f testsuite/TESTS/catalog.xml
	unzip -n $(TEST_ARCHIVE)
	patch testsuite/TESTS/catalog.xml catalog.diff
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

run-test-xslt : xslttest.exe
	$(RUNTIME) xslttest.exe --report:TestResult.xml --xml --details --outall

run-test-ms : alltest.exe xmlnorm.exe
	$(RUNTIME) ./alltest.exe $(TARGET_RESULTS)
	$(RUNTIME) ./xmlnorm.exe -s testsuite/TESTS/$(TARGET_RESULTS) testsuite/TESTS/norm-tmp
	rm -rf testsuite/TESTS/$(TARGET_RESULTS)
	mv testsuite/TESTS/norm-tmp testsuite/TESTS/$(TARGET_RESULTS)
	diff -r -q testsuite/TESTS/ExpectedResults testsuite/TESTS/$(TARGET_RESULTS) > diffreport.txt

clean : clean-xslttest clean-mstest

clean-xslttest :
	if (test -e testsuite) then \
		find testsuite/TESTS -name *.out > output.lst; \
		perl cleanup-output.pl; \
	fi
	rm -f xslttest.exe TestResult.xml output.lst

clean-mstest :
	rm -rf testsuite/TESTS/Reports
	rm -f diffreport.txt alltest.exe xmlnorm.exe

# Be careful to use it!
distclean :
	make clean
	rm -rf testsuite
	rm -f $(TEST_ARCHIVE) $(XSLTTEST_RESULTS_ARCHIVE) $(MSTEST_RESULTS_ARCHIVE)

create-reference-output : clean-original xslttest.exe
	./xslttest.exe --generate
	find testsuite/TESTS -name *.out > output.lst
	zip xslttest-reference-output.zip -@ < output.lst

