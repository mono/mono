thisdir = class/Mono.Data.Sqlite
SUBDIRS =
include ../../build/rules.make

RESX_RESOURCES = resources/SR.resources
OTHER_RESOURCES = $(wildcard resources/*.bmp)

RESOURCES = $(RESX_RESOURCES) $(OTHER_RESOURCES)

PREBUILT = $(RESX_RESOURCES:=.prebuilt)

LIBRARY = Mono.Data.Sqlite.dll
LIB_REFS = System System.Data System.Transactions System.Xml
KEYFILE = ../mono.pub
LIB_MCS_FLAGS = /unsafe \
	$(RESOURCES:%=-resource:%) -d:SQLITE_STANDARD

TEST_RESOURCE_FILES = Test/test.db

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /nowarn:618 $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

EXTRA_DISTFILES = Test/SqliteTest.cs \
	Test/test.sql \
	$(TEST_RESOURCE_FILES) \
	resources/SR.resx	\
	resources/DataTypes.xml	\
	resources/MetaDataCollections.xml \
	$(PREBUILT) \
	$(wildcard resources/*.bmp)

CLEAN_FILES += $(RESX_RESOURCES)

CLEAN_FILES = $(RESX_RESOURCES)

Test/test.db: Test/test.sql
	rm -f $@
	sqlite3 $@ < $<

include ../../build/library.make

$(the_lib): $(RESOURCES)

$(PREBUILT): %.prebuilt: %
	cp $* $@

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) -useSourcePath $< || cp $@.prebuilt $@

