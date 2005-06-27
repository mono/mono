RUNTIME = mono
MCS_RUNTIME =
MCS = mcs

TESTS = relax-ng/001/i.rng
TEST_ARCHIVE = testSuite.zip

RNCTESTS = test/RNCTest.xml
RNCTEST_ARCHIVE = anglia-test-suite.zip

all : relaxngtest.exe anglia-test-runner.exe

relaxngtest.exe : relaxngtest.cs $(TESTS)
	$(MCS_RUNTIME) $(MCS) -debug+ relaxngtest.cs -r:Commons.Xml.Relaxng.dll

anglia-test-runner.exe : anglia-test-runner.cs $(RNCTESTS)
	$(MCS_RUNTIME) $(MCS) -debug+ anglia-test-runner.cs -r:Commons.Xml.Relaxng.dll

$(TESTS) : $(TEST_ARCHIVE) relax-ng
	cd relax-ng; unzip -n ../$(TEST_ARCHIVE); \
	xsltproc split.xsl spectest.xml; cd ..; \
	cp prep.xsl relax-ng

$(RNCTESTS) : $(RNCTEST_ARCHIVE)
	unzip -n anglia-test-suite.zip

$(TEST_ARCHIVE) :
	wget http://www.thaiopensource.com/relaxng/testSuite.zip

$(RNCTEST_ARCHIVE) :
	wget http://www.jeffrafter.com/anglia/anglia-test-suite.zip

relax-ng:
	mkdir relax-ng

test : relaxngtest.exe
	$(RUNTIME) $(RUNTIME_FLAGS) relaxngtest.exe

anglia-test : anglia-test-runner.exe
	$(RUNTIME) $(RUNTIME_FLAGS) anglia-test-runner.exe

# be careful to use it. This removes ALL files in xml-test-suite!
clean :
#	rm -rf xml-test-suite
	rm relaxngtest.exe anglia-test-runner.exe
