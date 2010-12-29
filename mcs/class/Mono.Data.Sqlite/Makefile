thisdir = class/Mono.Data.Sqlite
SUBDIRS =
include ../../build/rules.make

RESX_RESOURCES = resources/SR.resources
OTHER_RESOURCES = $(wildcard resources/*.bmp)

RESOURCES = $(RESX_RESOURCES) $(OTHER_RESOURCES)

LIBRARY = Mono.Data.Sqlite.dll
LIB_MCS_FLAGS = /unsafe /r:System.dll /r:System.Data.dll /r:System.Transactions.dll /r:System.Xml.dll \
	$(RESOURCES:%=-resource:%) -d:SQLITE_STANDARD

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /nowarn:618

EXTRA_DISTFILES = Test/SqliteTest.cs \
	Test/test.sql \
	resources/SR.resx	\
	resources/ChangeLog	\
	resources/DataTypes.xml	\
	resources/MetaDataCollections.xml \
	$(wildcard resources/*.bmp)

test-local: test.db 

test.db: Test/test.sql
	rm -f $@
	sqlite3 $@ < $< || :

include ../../build/library.make

$(the_lib): $(RESOURCES)

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $<
