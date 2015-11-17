thisdir = class/System.Threading.Tasks.Dataflow
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Threading.Tasks.Dataflow.dll

include ../../build/library.make

LIB_REFS += System.Core System
LIB_MCS_FLAGS += -r:$(corlib) -d:CONCURRENT_COLLECTIONS

TEST_MCS_FLAGS = -r:System.Core.dll -r:System.dll

EXTRA_DISTFILES=README.md
