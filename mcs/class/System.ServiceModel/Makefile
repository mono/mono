thisdir = class/System.ServiceModel
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = \
	resources/WS-Addressing.schema	\
	resources/ws-addr.xsd

LIBRARY = System.ServiceModel.dll

LIB_REFS = System System.Xml System.Core System.Runtime.Serialization
LIB_MCS_FLAGS = \
		/nowarn:414,169,67,3005,436,219,618 \
		/unsafe \
		/d:TRACE \
		$(RESOURCE_FILES:%=/resource:%)

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Configuration System.Data System.Security System.IdentityModel System.IdentityModel.Selectors System.Transactions System.Messaging System.Web System.Web.Services Mono.Security
LIB_MCS_FLAGS += /d:NET_3_0
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += System.Web.ApplicationServices
activation = $(the_libdir_base)System.ServiceModel.Activation.dll
servicemodel_deps = $(activation)

LIB_REFS += System.Web.ApplicationServices

ifneq (plainservice/,$(intermediate))
LIB_MCS_FLAGS += -define:HAS_ACTIVATION -r:System.ServiceModel.Activation.dll
endif
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

$(the_libdir_base)System.ServiceModel.dll: $(servicemodel_deps)

$(activation):
	(cd ../System.ServiceModel.Activation; $(MAKE) $@)

ifneq (plainservice/,$(intermediate))
$(the_libdir_base)plainservice/System.ServiceModel.dll:
	$(MAKE) intermediate=plainservice/ $(the_libdir_base)plainservice/System.ServiceModel.dll
endif

.NOTPARALLEL: $(servicemodel_deps)

CLEAN_FILES = $(the_libdir_base)plainservice/System.ServiceModel.dll

ifndef intermediate
csproj-local:
	$(MAKE) csproj-local intermediate=plainservice/
endif
