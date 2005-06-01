.SUFFIXES: .cs .exe

RUNTIME=mono
XMLCONF_OPTIONS=
CSCOMPILE=mcs
TEST_ARCHIVE=xmlts20031210.zip
TEST_CATALOG=xmlconf/xmlconf.xml
TEST_PROG=xmlconf.exe

test: $(TEST_PROG) $(TEST_CATALOG)

run-test: test
	$(RUNTIME) $(TEST_PROG) $(XMLCONF_OPTIONS)

test_archive: $(TEST_ARCHIVE)

test_catalog: $(TEST_CATALOG)

$(TEST_ARCHIVE):
	wget http://www.w3.org/XML/Test/xmlts20031210.zip

$(TEST_CATALOG): $(TEST_ARCHIVE)
	unzip -un $(TEST_ARCHIVE)
	touch $(TEST_CATALOG)

.cs.exe :
	$(CSCOMPILE) $<

