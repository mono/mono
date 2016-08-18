thisdir = class/System.ServiceModel
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = \
	resources/WS-Addressing.schema	\
	resources/ws-addr.xsd

LIBRARY = System.ServiceModel.dll

LIB_REFS = System System.Xml System.Core System.Runtime.Serialization System.IdentityModel
LIB_MCS_FLAGS = \
		/nowarn:414,169,67,3005,436,219,618 \
		/unsafe \
		/d:TRACE \
		$(RESOURCE_FILES:%=/resource:%)

ifdef XAMMAC_4_5
LIB_REFS += System.ServiceModel.Internals
endif

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Configuration System.Data System.Security System.IdentityModel.Selectors System.Transactions System.Messaging System.Web.Services Mono.Security
LIB_MCS_FLAGS += /d:NET_3_0

ifndef NO_SYSTEM_WEB_DEPENDENCY
LIB_REFS += System.Web
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))

ifndef NO_SYSTEM_SERVICEMODEL_ACTIVATION_DEPENDENCY
activation = $(the_libdir_base)System.ServiceModel.Activation.dll
servicemodel_deps = $(activation)

ifneq (plainservice/,$(intermediate))
LIB_REFS += System.ServiceModel.Activation
LIB_MCS_FLAGS += -define:HAS_ACTIVATION
endif 
endif # NO_SYSTEM_SERVICEMODEL_ACTIVATION_DEPENDENCY

ifndef NO_SYSTEM_WEB_APPSERVICES_DEPENDENCY
LIB_REFS += System.Web.ApplicationServices
endif

endif # (4, $(FRAMEWORK_VERSION_MAJOR))

endif # (2.1, $(FRAMEWORK_VERSION))

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
