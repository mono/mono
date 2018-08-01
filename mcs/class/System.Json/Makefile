thisdir = class/System.Json
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Json.dll
LIB_REFS = System System.Xml System.Core
KEYFILE = ../winfx.pub
LIB_MCS_FLAGS = \
	-nowarn:3021

XTEST_LIB_REFS = System System.Core System.Xml Facades/System.Threading.Tasks

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Json/src/Resources/Strings.resx

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

include ../../build/library.make
