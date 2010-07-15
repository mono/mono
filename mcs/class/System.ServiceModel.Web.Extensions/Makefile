thisdir = class/System.ServiceModel.Web.Extensions
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Web.Extensions.dll
LIB_MCS_FLAGS = -r:System.dll -r:System.Xml.dll -r:System.Runtime.Serialization.dll -r:System.ServiceModel.dll -r:System.Core.dll -r:System.ServiceModel.Web.dll

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /r:System.Xml.Serialization.dll /r:System.Net.dll
endif

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /r:System.Configuration.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 2.1, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ServiceModel.Web.Extensions.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
endif

include ../../build/library.make
