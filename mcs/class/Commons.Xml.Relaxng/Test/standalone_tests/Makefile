RUNTIME = mono
MCS_RUNTIME =
MCS = mcs
TESTS = relax-ng/001/i.rng
TEST_ARCHIVE = testSuite.zip

relaxngtest.exe : relaxngtest.cs $(TESTS)
	$(MCS_RUNTIME) $(MCS) relaxngtest.cs -r:Commons.Xml.Relaxng.dll

$(TESTS) : $(TEST_ARCHIVE) relax-ng
	cd relax-ng; unzip -n ../$(TEST_ARCHIVE); \
	xsltproc split.xsl spectest.xml; cd ..; \
	cp prep.xsl relax-ng

$(TEST_ARCHIVE) :
	wget http://www.thaiopensource.com/relaxng/testSuite.zip

relax-ng:
	mkdir relax-ng

test :
	$(RUNTIME) relaxngtest.exe

# be careful to use it. This removes ALL files in xml-test-suite!
# clean:
#	rm -rf xml-test-suite
