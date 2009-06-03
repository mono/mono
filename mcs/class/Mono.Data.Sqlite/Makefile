thisdir = class/Mono.Data.Sqlite
SUBDIRS =
include ../../build/rules.make

ifneq (1, $(FRAMEWORK_VERSION_MAJOR))
OTHER_LIB_MCS_FLAGS = /r:System.Transactions.dll

RESX_RESOURCES = resources/SR.resources

RESOURCES = $(RESX_RESOURCES)
endif

LIBRARY = Mono.Data.Sqlite.dll
LIB_MCS_FLAGS = /unsafe /r:System.dll /r:System.Data.dll $(OTHER_LIB_MCS_FLAGS) \
	$(RESX_RESOURCES:%=-resource:%) -d:MONO_BACKWARD_COMPAT

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /nowarn:618

EXTRA_DISTFILES = Test/SqliteTest.cs \
	Test/test.sql \
	resources/SR.resx	\
	resources/ChangeLog	\
	resources/DataTypes.xml	\
	resources/MetaDataCollections.xml

run-test-local: test.db 

test.db: Test/test.sql
	rm -f $@
	sqlite3 $@ < $< || :

include ../../build/library.make
ifeq (net_2_0, $(PROFILE))
$(the_lib): $(RESOURCES)

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $<
endif

