thisdir = class/System.ServiceModel.Web
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Web.dll
LIB_REFS = System System.Xml System.Runtime.Serialization System.ServiceModel System.Core
LIB_MCS_FLAGS =

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Configuration 

ifndef NO_SYSTEM_WEB_DEPENDENCY
LIB_REFS += System.Web.Extensions
endif

LIB_MCS_FLAGS += -d:NET_3_5 -d:NET_3_0
endif

ifndef NO_SYSTEM_SERVICEMODEL_ACTIVATION_DEPENDENCY
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += System.ServiceModel.Activation
endif
endif

ifdef MOBILE_PROFILE
KEYFILE = ../silverlight.pub
else
KEYFILE = ../winfx.pub
endif

TEST_RESOURCE_FILES = \
	Test/config/webHttpBinding \
	Test/config/webHttpBinding.config

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_RESOURCE_FILES)

VALID_PROFILE := $(filter 2 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ServiceModel.Web.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
endif

test-data-ondotnet:
	csc -out:SyndicationSerializerUtil.exe -r:System.ServiceModel.Web.dll $(LIB_MCS_FLAGS) Test\\System.ServiceModel.Syndication\\SyndicationSerializerUtil.cs Test\\System.ServiceModel.Syndication\\FeedLib.cs
	./SyndicationSerializerUtil.exe

test-data:
	$(LIBRARY_COMPILE) Test/System.ServiceModel.Syndication/SyndicationSerializerUtil.cs Test/System.ServiceModel.Syndication/FeedLib.cs -out:SyndicationSerializerUtil.exe -r:System.ServiceModel.Web.dll $(LIB_MCS_FLAGS)
	./SyndicationSerializerUtil.exe

include ../../build/library.make
