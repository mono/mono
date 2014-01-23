thisdir = class/System.Json
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Json.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Xml.dll \
	        /r:System.Core.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

VALID_PROFILE := $(filter 2 4, $(FRAMEWORK_VERSION_MAJOR))

ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Json.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
