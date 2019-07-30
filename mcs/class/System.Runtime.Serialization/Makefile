thisdir = class/System.Runtime.Serialization
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES =

LIBRARY = System.Runtime.Serialization.dll
LIB_REFS = System System.Xml System.Core System.ServiceModel.Internals
LIB_MCS_FLAGS = \
		-unsafe \
		-d:NO_DYNAMIC_CODEGEN \
		/nowarn:168,169,219,414,618,1634 \
		$(RESOURCE_FILES:%=/resource:%)
TXT_RESOURCE_STRINGS = ../referencesource/System.Runtime.Serialization/System.Runtime.Serialization.txt

XTEST_LIB_REFS = System System.Core System.Xml System.Xml.Linq Facades/System.Threading.Tasks

ifneq (,$(filter build net_4_x, $(PROFILE)))
XTEST_LIB_REFS += System.Drawing
endif

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Data System.Configuration SMDiagnostics
LIB_MCS_FLAGS += /d:NET_3_0
else
LIB_MCS_FLAGS += /d:NO_CONFIGURATION /d:NO_SECURITY_ATTRIBUTES /d:NO_CODEDOM /d:NO_DESKTOP_SECURITY
endif

ifdef XAMMAC_4_5
LIB_MCS_FLAGS += /d:NO_CONFIGURATION
endif

TEST_RESOURCE_FILES = \
	$(wildcard Test/Resources/FrameworkTypes/*.xml) \
	$(wildcard Test/Resources/Schemas/*.xsd) \
	Test/Resources/WSDL/collections.wsdl \
	Test/Resources/WSDL/custom-collections.wsdl \
	Test/Resources/one.xml

# we need to write the test resource args into a response file to avoid
# hitting "argument list too long" errors because of too many files
TEST_RESOURCES_RESPONSE_FILE = $(depsdir)/$(LIBRARY)_test.resources.response

$(TEST_RESOURCES_RESPONSE_FILE): $(TEST_RESOURCE_FILES)
	@echo Creating $@ ...
	@echo $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r)) > $(TEST_RESOURCES_RESPONSE_FILE)

test-local: $(TEST_RESOURCES_RESPONSE_FILE)

CLEAN_FILES += $(TEST_RESOURCES_RESPONSE_FILE)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) @$(TEST_RESOURCES_RESPONSE_FILE)

ifneq (wasm, $(PROFILE))
TEST_LIB_REFS = System.ServiceModel System.Web.Services
endif

EXTRA_DISTFILES = $(RESOURCE_FILES) $(TEST_RESOURCE_FILES)

include ../../build/library.make
