thisdir = class/Mono.ServiceModel.IdentitySelectors
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = \
		resources/schemas-xmlsoap-or-ws-2005-05-identity.rnc

LIBRARY = Mono.ServiceModel.IdentitySelectors.dll
LIB_REFS = System System.Xml System.Configuration System.Security System.Transactions System.Web System.Web.Services Mono.Security
KEYFILE = ../mono.pub
LIB_MCS_FLAGS = \
		/nowarn:414,169,67,3005,436 \
		/unsafe \
		$(RESOURCE_FILES:%=/resource:%)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

TEST_EXT_RESOURCES = \
		Test/resources/managed.xml \
		Test/resources/rupert.crds \
		Test/resources/rupert.xml

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_EXT_REOURCES)

# This is a WinFX only assembly
VALID_PROFILE := $(filter net_3_0, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-Mono.ServiceModel.IdentitySelectors.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
