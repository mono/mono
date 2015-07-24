thisdir = class/Mono.Simd
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Simd.dll
LIB_REFS = System.Core
LIB_MCS_FLAGS = -r:$(corlib) /unsafe
TEST_MCS_FLAGS = -r:Mono.Simd.dll

NO_TEST = yes

include ../../build/library.make

