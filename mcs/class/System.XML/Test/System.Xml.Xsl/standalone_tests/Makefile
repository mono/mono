#
# "make" prepares automated download and build for all tests.
#
# "make run-test" executes both existing xslttest and MainSoft's new test
# and reports the total diff numbers:
#
#	- For xslttest, it is "TestResult.xml" and the list of the failed tests
#	is in failed.lst
#

.SUFFIXES: .cs .exe .dll

RUNTIME = mono

nunit_MONO_PATH="../../../../../class/lib/net_4_5"
CSCOMPILE=MONO_PATH=../../../../../class/lib/net_4_5 ../../../../../../runtime/mono-wrapper ../../../../../class/lib/build/mcs.exe

TEST_ARCHIVE = xslt-testsuite-03.ZIP
CATALOG = testsuite/TESTS/catalog.xml
FIXED_CATALOG = testsuite/TESTS/catalog-fixed.xml

mono_wrapper_PATH="../../../../../../runtime/mono-wrapper"
NUNIT_CONSOLE=$(mono_wrapper_PATH) --debug $(nunit_MONO_PATH)/nunit-console.exe

FIXTURE=MonoTests.oasis_xslt.SuiteBuilder
CLEAN_NUNIT_FLAGS=-fixture:$(FIXTURE) -xml=TestResult.xml -include=Clean
FAILED_NUNIT_FLAGS=-fixture:$(FIXTURE) -xml=FailedTestResult.xml -include=KnownFailures
ALL_NUNIT_FLAGS=-fixture:$(FIXTURE) -xml=AllTestResult.xml -out=AllTestResult.log -include=KnownFailures,Clean

TEST_PROG=xslttest.dll
GENERATE_EXE=generate.exe

ifdef TEST_DOM
REFERENCE_RESULTS_NAME=domresults
else
REFERENCE_RESULTS_NAME=results
endif
REFERENCE_RESULTS_ARCHIVE=xslt-reference-$(REFERENCE_RESULTS_NAME).tar.gz
REFERENCE_RESULTS_URL=https://github.com/mono/release/raw/master/test-ext/xslt-standalone/$(REFERENCE_RESULTS_ARCHIVE)
REFERENCE_RESULTS=$(REFERENCE_RESULTS_NAME)/timestamp

test : $(TEST_PROG) $(FIXED_CATALOG) $(REFERENCE_RESULTS)

run-test : $(TEST_PROG) $(FIXED_CATALOG) $(REFERENCE_RESULTS)
	MONO_PATH="$(nunit_MONO_PATH);$(MONO_PATH)" $(NUNIT_CONSOLE) $(TEST_PROG) $(CLEAN_NUNIT_FLAGS)
	
run-all-test : $(TEST_PROG) $(FIXED_CATALOG) $(REFERENCE_RESULTS)
	MONO_PATH="$(nunit_MONO_PATH);$(MONO_PATH)" $(NUNIT_CONSOLE) $(TEST_PROG) $(ALL_NUNIT_FLAGS)
	
run-failed-test : $(TEST_PROG) $(FIXED_CATALOG) $(REFERENCE_RESULTS)
	XSLTTEST_INVERSE_RESULTS="" MONO_PATH="$(nunit_MONO_PATH);$(MONO_PATH)" $(NUNIT_CONSOLE) $(TEST_PROG) $(FAILED_NUNIT_FLAGS)
	

clean :
	rm -f *TestResult.xml failed.lst missing.lst AllTestResult.log
	rm -f $(TEST_PROG) $(GENERATE_EXE)

# Be careful to use it!
distclean : clean
	rm -rf testsuite
	rm -rf results domresults
	rm -f $(TEST_ARCHIVE) $(REFERENCE_RESULTS_ARCHIVE) $(REFERENCE_LIST)

# Check that we are running on MS.NET - otherwise the reference output can be
# created on mono - and we will compare mono with mono
must-be-dotnet:
ifdef GENERATE_REFERENCE_ON_MONO
	true
else
	uname | grep CYGWIN || uname | grep Windows
endif
	
create-reference-output : must-be-dotnet $(GENERATE_EXE) $(FIXED_CATALOG)
	rm -rf $(REFERENCE_RESULTS_NAME)
ifdef GENERATE_REFERENCE_ON_MONO
	$(RUNTIME) ./$(GENERATE_EXE) $(TEST_DOM)
else
	./generate.exe $(TEST_DOM)
endif
# Must cd to work with any path separator symbols
	cd $(REFERENCE_RESULTS_NAME); echo "$(TEST_DOM)" > generate_options 
	tar -c $(REFERENCE_RESULTS_NAME) | gzip > $(REFERENCE_RESULTS_ARCHIVE)
	@echo "Now you can upload $(REFERENCE_RESULTS_ARCHIVE) to $(REFERENCE_RESULTS_URL)"

$(GENERATE_EXE) : generate.cs XsltTestUtils.cs
	$(CSCOMPILE) generate.cs XsltTestUtils.cs -out:$@
	# It somehow does not work fine unless nunit-console.exe is here ...
	cp $(nunit_MONO_PATH)/nunit-console.exe .
$(TEST_PROG) : xslttest.cs XsltTestUtils.cs
	$(CSCOMPILE) xslttest.cs XsltTestUtils.cs XmlCompare.cs -r:nunit.core -r:nunit.core.interfaces -r:nunit.framework -out:$@ -t:library

catalog-fixed : $(FIXED_CATALOG)

$(FIXED_CATALOG) : $(CATALOG) catalog.sed catalog-fixed.diff
	sed -f catalog.sed $(CATALOG) > $(FIXED_CATALOG)
	patch -p0 -i catalog-fixed.diff

catalog : $(CATALOG)

$(CATALOG) : $(TEST_ARCHIVE)
	unzip -un $(TEST_ARCHIVE)
	touch $(CATALOG)

$(TEST_ARCHIVE) :
	wget http://www.oasis-open.org/committees/download.php/9584/$(TEST_ARCHIVE)

$(REFERENCE_RESULTS) : $(REFERENCE_RESULTS_ARCHIVE)
	tar -xzf $<
	touch $@

$(REFERENCE_RESULTS_ARCHIVE) :
	wget -O $(REFERENCE_RESULTS_ARCHIVE) $(REFERENCE_RESULTS_URL)

