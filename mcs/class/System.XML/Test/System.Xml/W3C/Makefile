.SUFFIXES: .cs .exe .dll

RUNTIME=mono
XMLCONF_OPTIONS=

ifeq ($(PROFILE), net_2_0)
	nunit_MONO_PATH="../../../../../class/lib/net_2_0"
	CSCOMPILE=gmcs
else
	nunit_MONO_PATH="../../../../../class/lib/net_1_1"
	CSCOMPILE=mcs
endif

REFERENCES=-d:NUNIT_SUPPORT -r:nunit.core -r:nunit.framework
TEST_ARCHIVE=xmlts20031210.zip
TEST_CATALOG=xmlconf/xmlconf.xml
TEST_PROG=xmlconf.dll
mono_wrapper_PATH="../../../../../../mono/runtime/mono-wrapper"
NUNIT_CONSOLE=$(mono_wrapper_PATH) --debug $(nunit_MONO_PATH)/nunit-console.exe

CLEAN_FIXTURE=MonoTests.W3C_xmlconf.CleanTests
CLEAN_NUNIT_FLAGS=/fixture:$(CLEAN_FIXTURE) /xml=TestsResult.xml

FAILING_FIXTURE=MonoTests.W3C_xmlconf.KnownFailureTests
FAILING_NUNIT_FLAGS=/fixture:$(FAILING_FIXTURE) /xml=FailingTestsResult.xml

ALL_FIXTURE=MonoTests.W3C_xmlconf.AllTests
ALL_NUNIT_FLAGS=/fixture:$(ALL_FIXTURE) /xml=AllTestsResult.xml /out=AllTestsResult.log



test: $(TEST_PROG) $(TEST_CATALOG)

run-test: test
	MONO_PATH="$(nunit_MONO_PATH);$(MONO_PATH)" $(NUNIT_CONSOLE) $(TEST_PROG) $(CLEAN_NUNIT_FLAGS)

run-failing-test: test
	MONO_PATH="$(nunit_MONO_PATH);$(MONO_PATH)" $(NUNIT_CONSOLE) $(TEST_PROG) $(FAILING_NUNIT_FLAGS)

run-all-test: test
	MONO_PATH="$(nunit_MONO_PATH);$(MONO_PATH)" $(NUNIT_CONSOLE) $(TEST_PROG) $(ALL_NUNIT_FLAGS)

test_archive: $(TEST_ARCHIVE)

test_catalog: $(TEST_CATALOG)

$(TEST_ARCHIVE):
	wget http://www.w3.org/XML/Test/xmlts20031210.zip

$(TEST_CATALOG): $(TEST_ARCHIVE)
	mkdir xmlconf; unzip -un $(TEST_ARCHIVE)
	touch $(TEST_CATALOG)

.cs.dll:
	$(CSCOMPILE) /t:library /lib:$(nunit_MONO_PATH) $< $(REFERENCES)

