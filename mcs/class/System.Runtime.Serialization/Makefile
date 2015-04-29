thisdir = class/System.Runtime.Serialization
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES =

LIBRARY = System.Runtime.Serialization.dll
LIB_REFS = System System.Xml System.Core System.ServiceModel.Internals SMDiagnostics
LIB_MCS_FLAGS = \
		-unsafe \
		-d:NO_DYNAMIC_CODEGEN \
		/nowarn:168,169,219,414 \
		$(RESOURCE_FILES:%=/resource:%)

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Data System.Configuration
LIB_MCS_FLAGS += /d:NET_3_0
else
LIB_MCS_FLAGS += /d:NO_CONFIGURATION /d:NO_SECURITY_ATTRIBUTES /d:NO_CODEDOM /d:NO_DESKTOP_SECURITY
endif

TEST_RESOURCE_FILES = \
	Test/Resources/WSDL/collections.wsdl	\
	Test/Resources/WSDL/custom-collections.wsdl

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /r:System.ServiceModel.dll /r:System.Web.Services.dll \
	$(TEST_RESOURCE_FILES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_RESOURCE_FILES) \
	Test/Resources/FrameworkTypes/* \
	Test/Resources/Schemas/*.xsd \
	Test/System.Runtime.Serialization/one.xml \
	ReferenceSource.common.sources \
	ReferenceSource.desktop.sources

include ../../build/library.make
