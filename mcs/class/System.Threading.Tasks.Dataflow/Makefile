thisdir = class/System.Threading.Tasks.Dataflow
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Threading.Tasks.Dataflow.dll

include ../../build/library.make

LIB_REFS += System.Core System
LIB_MCS_FLAGS += -d:CONCURRENT_COLLECTIONS

TEST_MCS_FLAGS =
TEST_LIB_REFS = System.Core System

EXTRA_DISTFILES=README.md
