thisdir = class/System.ServiceModel
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = \
	resources/WS-Addressing.schema	\
	resources/ws-addr.xsd

LIBRARY = System.ServiceModel.dll
LIB_MCS_FLAGS = \
		/nowarn:414,169,67,3005,436,219,618 \
		/unsafe \
		/d:TRACE \
	        /r:System.dll \
	        /r:System.Xml.dll \
		/r:System.Core.dll \
	        /r:System.Runtime.Serialization.dll \
		$(RESOURCE_FILES:%=/resource:%)

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /d:NET_3_0 	\
			/r:System.Configuration.dll \
			/r:System.Data.dll	\
			/r:System.Security.dll \
			/r:System.IdentityModel.dll \
			/r:System.IdentityModel.Selectors.dll \
			/r:System.Transactions.dll \
			/r:System.Messaging.dll \
			/r:System.Web.dll \
			/r:System.Web.Services.dll \
			/r:Mono.Security.dll
endif

ifeq (moonlight_raw, $(PROFILE))
LIB_MCS_FLAGS += /r:System.Net
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += /r:System.Web.ApplicationServices.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

TEST_EXTRA_FILES = \
	System.ServiceModel_test_net_3_0.dll.config \
	Test/config/* \
	Test/Resources/* \
	Test/XmlFiles/* \
	Test/System.ServiceModel.Channels/soap-fault*.xml \
	Test/System.ServiceModel.Channels/binary-message.raw \
	Test/System.ServiceModel.Description/dump.xml \
	Test/MetadataTests/Resources/*

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_EXTRA_FILES)

# Useful for debugging under Visual Studio 2005
# NO_SIGN_ASSEMBLY = yes

VALID_PROFILE := $(filter 2 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ServiceModel.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif



include ../../build/library.make
