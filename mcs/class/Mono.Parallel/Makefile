thisdir = class/Mono.Parallel
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Parallel.dll

include ../../build/library.make

TEST_MCS_FLAGS = /r:System.Core.dll

LIB_REFS += System.Core System
LIB_MCS_FLAGS += -d:INSIDE_MONO_PARALLEL -r:$(corlib)
