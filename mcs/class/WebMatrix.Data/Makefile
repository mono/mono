thisdir = class/WebMatrix.Data
SUBDIRS = 
include ../../build/rules.make

LIBRARY = WebMatrix.Data.dll

LIB_REFS = System System.Data System.Core System.Configuration
KEYFILE = ../mono.pub
LIB_MCS_FLAGS =
TEST_RESOURCE_FILES = Test/resources/testsqlite.db
TEST_MCS_FLAGS = $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))
TEST_LIB_REFS = System System.Core System.Data Mono.Data.Sqlite Microsoft.CSharp

EXTRA_DISTFILES = $(TEST_RESOURCE_FILES)

include ../../build/library.make
