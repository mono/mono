RUNTIME = mono
MCS_RUNTIME =
MCS = mcs
TESTS = xsd-test-suite/suntest/SunTestsAll/xsiType1.xsd
TEST_ARCHIVE = xmlts20031210.zip

xmltest.exe : xmltest.cs $(TESTS)
	$(MCS_RUNTIME) $(MCS) xmltest.cs

$(TESTS) : $(TEST_ARCHIVE) xml-test-suite
	cd xml-test-suite; unzip -n ../$(TEST_ARCHIVE); cd ..

$(TEST_ARCHIVE) :
	wget http://www.w3.org/XML/Test/xmlts20031210.zip

xml-test-suite:
	mkdir xml-test-suite

test :
	$(RUNTIME) xmltest.exe

# be careful to use it. This removes ALL files in xml-test-suite!
# clean:
#	rm -rf xml-test-suite
