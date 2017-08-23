thisdir = class/System.Threading.Tasks.Dataflow
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Threading.Tasks.Dataflow.dll

LIB_REFS += System.Core System
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS += -d:CONCURRENT_COLLECTIONS

include ../../build/library.make

TEST_MCS_FLAGS =
TEST_LIB_REFS = System.Core System

EXTRA_DISTFILES=README.md
