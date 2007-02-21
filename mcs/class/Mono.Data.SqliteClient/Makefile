thisdir = class/Mono.Data.SqliteClient
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.Data.SqliteClient.dll
LIB_MCS_FLAGS = /unsafe /r:System.dll /r:System.Data.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /nowarn:618

EXTRA_DISTFILES = Test/SqliteTest.cs \
	Test/test.sql

run-test-local: test.db 

test.db: Test/test.sql
	rm -f $@
	sqlite3 $@ < $< || :

include ../../build/library.make
