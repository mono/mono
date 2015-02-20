thisdir = class/System.Runtime.Serialization
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES = \
	resources/mstypes.schema

LIBRARY = System.Runtime.Serialization.dll
LIB_MCS_FLAGS = \
		-unsafe \
		/nowarn:168,169,219,414 \
		/r:System.dll \
		/r:System.Xml.dll \
		/r:System.Core.dll \
		$(RESOURCE_FILES:%=/resource:%)

RESOURCE_STRINGS = ../../../external/referencesource/System.Runtime.Serialization/System.Runtime.Serialization.txt

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /d:NET_3_0 /r:System.Data.dll /r:System.Configuration.dll
endif

TEST_RESOURCE_FILES = \
	Test/Resources/WSDL/collections.wsdl	\
	Test/Resources/WSDL/custom-collections.wsdl

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /r:System.ServiceModel.dll /r:System.Web.Services.dll \
	$(TEST_RESOURCE_FILES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_RESOURCE_FILES) \
	Test/Resources/FrameworkTypes/* \
	Test/Resources/Schemas/*.xsd \
	Test/System.Runtime.Serialization/one.xml

include ../../build/library.make
