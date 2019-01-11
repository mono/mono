thisdir = class/System.IO.Compression
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.IO.Compression.dll
LIB_REFS = System
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS =
TEST_RESOURCE_FILES = Test/resources/archive.zip Test/resources/test.nupkg
TEST_MCS_FLAGS = $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))
TEST_LIB_REFS = System System.Core
LIBRARY_WARN_AS_ERROR = yes

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.IO.Compression/src/Resources/Strings.resx

EXTRA_DISTFILES = $(TEST_RESOURCE_FILES)

include ../../build/library.make
