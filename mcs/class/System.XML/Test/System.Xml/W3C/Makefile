.SUFFIXES: .cs .exe

RUNTIME=mono
XMLCONF_OPTIONS=
CSCOMPILE=mcs
ifndef NO_NUNIT
REFERENCES=-d:NUNIT_SUPPORT -r:NUnit.Core -r:NUnit.Framework
endif
TEST_ARCHIVE=xmlts20031210.zip
TEST_CATALOG=xmlconf/xmlconf.xml
TEST_PROG=xmlconf.exe

test: $(TEST_PROG) $(TEST_CATALOG)

run-test: test
	$(RUNTIME) $(TEST_PROG) $(XMLCONF_OPTIONS)

run-nunit-test: test
	MONO_PATH="../../../../../class/lib/default;;$(MONO_PATH)" /monobuild/mono/runtime/mono-wrapper --debug ../../../../../class/lib/default/nunit-console.exe $(TEST_PROG) /fixture:MonoTests.stand_alone.AllTests /xml=TestResult.xml /out=TestResult.log

test_archive: $(TEST_ARCHIVE)

test_catalog: $(TEST_CATALOG)

$(TEST_ARCHIVE):
	wget http://www.w3.org/XML/Test/xmlts20031210.zip

$(TEST_CATALOG): $(TEST_ARCHIVE)
	mkdir xmlconf; unzip -un $(TEST_ARCHIVE)
	touch $(TEST_CATALOG)

.cs.exe :
	$(CSCOMPILE) $< $(REFERENCES)

