thisdir = class/System.Xml.Linq
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.Linq.dll
LIB_MCS_FLAGS = \
	        -r:System.dll \
	        -r:System.Core.dll \
	        -r:System.Xml.dll

ifneq (2.1, $(FRAMEWORK_VERSION))
# This is a .NET 3.5+ only assembly, but built during the 2.0 build
LIB_MCS_FLAGS += -d:NET_3_5 -nowarn:1720
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

VALID_PROFILE := $(filter 2 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Xml.Linq.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
