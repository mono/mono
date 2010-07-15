thisdir = class/System.ServiceModel.Syndication
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Syndication.dll
LIB_MCS_FLAGS = -r:System.dll -r:System.Xml.dll -r:System.Runtime.Serialization.dll -r:System.ServiceModel.dll -r:System.Core.dll

ifeq (moonlight_raw, $(PROFILE))
LIB_MCS_FLAGS += /r:System.Xml.Serialization.dll
endif

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /r:System.Configuration.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 2.1, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ServiceModel.Syndication.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
endif

include ../../build/library.make
