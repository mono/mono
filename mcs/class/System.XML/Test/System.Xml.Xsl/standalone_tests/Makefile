#
# "make" prepares automated download and build for all tests.
#
# "make run-test" executes both existing xslttest and MainSoft's new test
# and reports the total diff numbers:
#
#	- For xslttest, it is "TestResult.xml" and the list of the failed tests
#	is in failed.lst
#

.SUFFIXES: .cs .exe

RUNTIME = mono
CSCOMPILE = mcs

TEST_ARCHIVE = xslt-testsuite-03.ZIP
CATALOG = testsuite/TESTS/catalog.xml
FIXED_CATALOG = testsuite/TESTS/catalog-fixed.xml

TEST_EXE = xslttest.exe
TEST_FLAGS = --report:TestResult.xml --xml --details --outall $(TEST_DOM) 

ifdef TEST_DOM
REFERENCE_RESULTS_NAME = domresults
else
REFERENCE_RESULTS_NAME = results
endif
REFERENCE_RESULTS_ARCHIVE = xslt-reference-$(REFERENCE_RESULTS_NAME).tar.gz
REFERENCE_RESULTS_URL = http://svn.myrealbox.com/source/trunk/release/test-ext/xslt-standalone/$(REFERENCE_RESULTS_ARCHIVE)
REFERENCE_RESULTS = $(REFERENCE_RESULTS_NAME)/timestamp

test : $(TEST_EXE) $(FIXED_CATALOG)

# Redirect stdout to /dev/null because it has only xsl:message garbage
run-test : $(TEST_EXE) $(FIXED_CATALOG) $(REFERENCE_RESULTS)
	$(RUNTIME) $(RUNTIME_FLAGS) $(TEST_EXE) $(TEST_FLAGS) >/dev/null

clean :
	rm -f TestResult.xml failed.lst missing.lst
	rm -f $(TEST_EXE)

# Be careful to use it!
distclean : clean
	rm -rf testsuite
	rm -f $(TEST_ARCHIVE) $(REFERENCE_RESULTS_ARCHIVE) $(REFERENCE_LIST)

# Check that we are running on MS.NET - otherwise the reference output can be
# created on mono - and we will compare mono with mono
must-be-dotnet:
ifdef GENERATE_REFERENCE_ON_MONO
	true
else
	uname | grep CYGWIN || uname | grep Windows
endif
	
create-reference-output : must-be-dotnet $(TEST_EXE)
	rm -rf $(REFERENCE_RESULTS_NAME)
ifdef GENERATE_REFERENCE_ON_MONO
	$(RUNTIME) ./$(TEST_EXE) $(TEST_DOM) --generate
else
	./$(TEST_EXE) $(TEST_DOM) --generate
endif
# Must cd to work with any path separator symbols
	cd $(REFERENCE_RESULTS_NAME); echo "$(TEST_DOM)" > generate_options 
	tar -c $(REFERENCE_RESULTS_NAME) | gzip > $(REFERENCE_RESULTS_ARCHIVE)
	@echo "Now you can upload $(REFERENCE_RESULTS_ARCHIVE) to $(REFERENCE_RESULTS_URL)"

.cs.exe :
	$(CSCOMPILE) $<

$(FIXED_CATALOG) : $(CATALOG) catalog.sed
	sed -f catalog.sed $(CATALOG) > $(FIXED_CATALOG)

$(CATALOG) : $(TEST_ARCHIVE)
	unzip -un $(TEST_ARCHIVE)
	touch $(CATALOG)

$(TEST_ARCHIVE) :
	wget http://www.oasis-open.org/committees/download.php/9584/$(TEST_ARCHIVE)

$(REFERENCE_RESULTS) : $(REFERENCE_RESULTS_ARCHIVE)
	tar -xzf $<
	touch $@

$(REFERENCE_RESULTS_ARCHIVE) :
	wget $(REFERENCE_RESULTS_URL)

