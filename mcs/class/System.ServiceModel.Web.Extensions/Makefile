thisdir = class/System.ServiceModel.Web.Extensions
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Web.Extensions.dll
LIB_REFS = System System.Xml System.Runtime.Serialization System.ServiceModel System.Core System.ServiceModel.Web
LIB_MCS_FLAGS =

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Xml.Serialization System.Net
endif

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Configuration
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
