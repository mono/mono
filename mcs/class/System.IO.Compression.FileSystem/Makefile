thisdir = class/System.IO.Compression.FileSystem
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.IO.Compression.FileSystem.dll
LIB_REFS = System System.IO.Compression
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS = -unsafe -nowarn:436

TEST_RESOURCE_FILES = \
	Test/resources/foo/foo.txt \
	Test/resources/foo/bar.txt \
	Test/resources/foo/foobar/foo.txt \
	Test/resources/foo/foobar/bar.txt

TEST_MCS_FLAGS = $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))
TEST_LIB_REFS = System System.Core System.IO.Compression
LIBRARY_WARN_AS_ERROR = yes

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.IO.Compression.ZipFile/src/Resources/Strings.resx

include ../../build/library.make
