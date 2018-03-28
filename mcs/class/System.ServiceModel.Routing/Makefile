thisdir = class/System.ServiceModel.Routing
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Routing.dll
LIB_REFS = System System.Xml System.Runtime.Serialization System.ServiceModel System.Core
LIB_MCS_FLAGS =

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Configuration
LIB_MCS_FLAGS += -d:NET_3_5 -d:NET_3_0
endif

ifdef MOBILE_PROFILE
KEYFILE = ../silverlight.pub
else
KEYFILE = ../winfx.pub
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 2 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ServiceModel.Routing.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
endif

include ../../build/library.make
