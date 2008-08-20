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
	        /r:System.dll \
	        /r:System.Xml.dll \
	        /r:System.Configuration.dll \
	        /r:../lib/net_3_0/System.Runtime.Serialization.dll \
	        /r:System.Security.dll \
	        /r:../lib/net_3_0/System.IdentityModel.dll \
		/r:../lib/net_3_0/System.IdentityModel.Selectors.dll \
	        /r:System.Transactions.dll \
		/r:System.Messaging.dll \
	        /r:System.Web.dll \
		/r:System.Web.Services.dll \
		/r:Mono.Security.dll \
		$(RESOURCE_FILES:%=/resource:%)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

TEST_EXTRA_FILES = \
	System.ServiceModel_test_net_3_0.dll.config \
	Test/config/* \
	Test/Resources/* \
	Test/System.ServiceModel.Description/dump.xml

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_EXTRA_FILES)

# This is a WinFX only assembly

# Useful for debugging under Visual Studio 2005
# NO_SIGN_ASSEMBLY = yes

VALID_PROFILE := $(filter net_3_0 net_3_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ServiceModel.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif


include ../../build/library.make
