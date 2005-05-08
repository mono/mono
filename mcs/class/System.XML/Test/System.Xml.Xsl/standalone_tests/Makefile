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

RUNTIME = mono --debug
CSCOMPILE = mcs
TESTS = testsuite/TESTS/Xalan_Conformance_Tests/whitespace/whitespace35.xsl
TARGET_RESULTS = Results
MSTEST_RESULTS = testsuite/TESTS/ExpectedResults
TEST_ARCHIVE = xslt-testsuite-03.ZIP
XSLTTEST_RESULTS_ARCHIVE = xslttest-reference-output.zip
MSTEST_RESULTS_ARCHIVE = mono-xslttest-ExpectedResults-20050214.zip

test : xslttest.exe alltest.exe prepare

prepare : $(TESTS) prepare-xslt prepare-mstest

prepare-xslt : xslttest.exe $(XSLTTEST_RESULTS) testsuite/TESTS/catalog-fixed.xml

prepare-mstest : alltest.exe $(MSTEST_RESULTS) testsuite/TESTS/catalog-out.xml

xslttest.exe : xslttest.cs
	$(CSCOMPILE) xslttest.cs

xmlnorm.exe : XmlNormalizer.cs
	$(CSCOMPILE) XmlNormalizer.cs -out:xmlnorm.exe

alltest.exe : alltest.cs
	$(CSCOMPILE) alltest.cs

testsuite/TESTS/catalog-fixed.xml : testsuite/TESTS/catalog.xml catalog.sed
	sed -f catalog.sed testsuite/TESTS/catalog.xml > testsuite/TESTS/catalog-fixed.xml

$(TESTS) : $(TEST_ARCHIVE) simplify.xsl
	unzip -un $(TEST_ARCHIVE)
	touch $(TESTS)

testsuite/TESTS/catalog-out.xml : testsuite/TESTS/catalog-fixed.xml simplify.xsl
	xsltproc simplify.xsl testsuite/TESTS/catalog-fixed.xml > testsuite/TESTS/catalog-out.xml

$(XSLTTEST_RESULTS) : $(XSLTTEST_RESULTS_ARCHIVE)
	unzip -n -d $(XSLTTEST_RESULTS_ARCHIVE)
	touch $(XSLTTEST_RESULTS)

$(MSTEST_RESULTS) : $(MSTEST_RESULTS_ARCHIVE)
	unzip -n -d testsuite/TESTS $(MSTEST_RESULTS_ARCHIVE)
	touch $(MSTEST_RESULTS)

$(TEST_ARCHIVE) :
	wget http://www.oasis-open.org/committees/download.php/9584/$(TEST_ARCHIVE)

$(XSLTTEST_RESULTS_ARCHIVE) :
	wget http://monkey.workarea.jp/mono/xml/$(XSLTTEST_RESULTS_ARCHIVE)

$(MSTEST_RESULTS_ARCHIVE) :
	wget http://monkey.workarea.jp/mono/xml/$(MSTEST_RESULTS_ARCHIVE)

run-test : run-test-xslt run-test-ms

run-test-xslt : xslttest.exe testsuite/TESTS/catalog-fixed.xml
	$(RUNTIME) $(RUNTIME_FLAGS) xslttest.exe --report:TestResult.xml --xml --details --outall

run-test-ms : alltest.exe xmlnorm.exe testsuite/TESTS/catalog-out.xml
	$(RUNTIME) $(RUNTIME_FLAGS) ./alltest.exe $(TARGET_RESULTS)
	mono ./xmlnorm.exe -s testsuite/TESTS/$(TARGET_RESULTS) testsuite/TESTS/norm-tmp
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

create-reference-output : clean-xslttest xslttest.exe
	./xslttest.exe --generate
	find testsuite/TESTS -name *.out > output.lst
	zip xslttest-reference-output.zip -@ < output.lst

