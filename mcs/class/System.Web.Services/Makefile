thisdir = class/System.Web.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Services.dll
KEYFILE = ../msfinal.pub
ifdef MOBILE_PROFILE
LIB_REFS = System System.Xml
LIB_MCS_FLAGS = \
	-nowarn:649 -nowarn:168		\
	-resource:System.Web.Services.Description/wsdl-1.1.xsd,wsdl-1.1.xsd \
	-resource:System.Web.Services.Description/wsdl-1.1-soap.xsd,wsdl-1.1-soap.xsd \
	-resource:System.Web.Services.Description/web-reference.xsd,web-reference.xsd
else
LIB_REFS = System System.EnterpriseServices System.Xml System.Data
LIB_MCS_FLAGS = -nowarn:168,612,618,649 -d:MONO_BROKEN_CONFIGURATION_DLL

ifndef NO_SYSTEM_WEB_DEPENDENCY
API_BIN_REFS := System.Web
endif

ifndef NO_SYSTEM_DESIGN_DEPENDENCY
LIB_REFS += System.Design
endif

ifndef NO_SYSTEM_DIRECTORY_SERVICES_DEPENDENCY
LIB_REFS += System.DirectoryServices
endif

LIB_REFS += System.Configuration
LIB_MCS_FLAGS += \
	-resource:System.Web.Services.Description/wsdl-1.1.xsd,wsdl-1.1.xsd \
	-resource:System.Web.Services.Description/wsdl-1.1-soap.xsd,wsdl-1.1-soap.xsd \
	-resource:System.Web.Services.Description/web-reference.xsd,web-reference.xsd
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618

ifndef NO_THREAD_ABORT
TEST_MCS_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
endif

ifndef NO_THREAD_SUSPEND_RESUME
TEST_MCS_FLAGS += -d:MONO_FEATURE_THREAD_SUSPEND_RESUME
endif

TEST_RESOURCE_FILES = $(wildcard Test/System.Web.Services.Description/*.wsdl)

TEST_MCS_FLAGS += $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

EXTRA_DISTFILES = \
	System.Web.Services.Description/web-reference.xsd	\
	System.Web.Services.Description/wsdl-1.1.xsd		\
	System.Web.Services.Description/wsdl-1.1-soap.xsd	\
	System.Web.Services.Description/wsdl.genxs		\
	$(TEST_RESOURCE_FILES)

include ../../build/library.make
