thisdir = class/Mono.Data.SqliteClient
SUBDIRS =
include ../../build/rules.make

ifeq (net_2_0, $(PROFILE))
OTHER_LIB_MCS_FLAGS = /r:System.Transactions.dll

RESX_RESOURCES = resources/SR.resources

RESOURCES = $(RESX_RESOURCES)
endif

LIBRARY = Mono.Data.SqliteClient.dll
LIB_MCS_FLAGS = /unsafe /r:System.dll /r:System.Data.dll $(OTHER_LIB_MCS_FLAGS) \
	$(RESX_RESOURCES:%=-resource:%) -d:MONO_BACKWARD_COMPAT

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /nowarn:618

EXTRA_DISTFILES = Test/SqliteTest.cs \
	Test/test.sql $(wildcard resources/*.cs)

run-test-local: test.db 

test.db: Test/test.sql
	rm -f $@
	sqlite3 $@ < $< || :

ifeq (net_2_0, $(PROFILE))
include ../../build/library.make
$(the_lib): $(RESOURCES)

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $<
endif

include ../../build/library.make
