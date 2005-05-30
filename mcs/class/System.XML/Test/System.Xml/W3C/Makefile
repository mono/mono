TEST_ARCHIVE=xmlts20031210.zip
TEST_CATALOG=xmlconf/xmlconf.xml

all:

test_archive: $(TEST_ARCHIVE)

test_catalog: $(TEST_CATALOG)

$(TEST_ARCHIVE):
	wget http://www.w3.org/XML/Test/xmlts20031210.zip

$(TEST_CATALOG): $(TEST_ARCHIVE)
	unzip -un $(TEST_ARCHIVE)
