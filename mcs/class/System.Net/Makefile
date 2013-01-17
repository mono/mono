thisdir = class/System.Net
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Net.dll
LIB_MCS_FLAGS =  -unsafe -r:System.dll -r:System.Core.dll -r:System.Xml.dll -d:NET_2_1 -d:NET_2_0 -d:NET_1_1

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_5 -nowarn:1720
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

VALID_PROFILE := $(filter 2 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Net.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
