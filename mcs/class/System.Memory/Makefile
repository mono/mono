thisdir = class/System.Memory
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Memory.dll
LIB_REFS = System System.Runtime.CompilerServices.Unsafe
LIB_MCS_FLAGS = /unsafe
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Memory/src/Resources/Strings.resx

include ../../build/library.make
