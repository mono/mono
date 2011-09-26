thisdir = class/System.ServiceModel.Web
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Web.dll
LIB_MCS_FLAGS = -r:System.dll -r:System.Xml.dll -r:System.Runtime.Serialization.dll -r:System.ServiceModel.dll -r:System.Core.dll

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_5 -d:NET_3_0 	\
			-r:System.Configuration.dll \
			-r:System.Web.Extensions.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES) \
	Test/config/webHttpBinding \
	Test/config/webHttpBinding.config

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
