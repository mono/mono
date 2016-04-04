thisdir = class/System.Web.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Services.dll
ifdef MOBILE_PROFILE
LIB_REFS = System System.Xml
LIB_MCS_FLAGS = \
	-nowarn:649 -nowarn:169			\
	-resource:System.Web.Services.Description/wsdl-1.1.xsd,wsdl-1.1.xsd \
	-resource:System.Web.Services.Description/wsdl-1.1-soap.xsd,wsdl-1.1-soap.xsd \
	-resource:System.Web.Services.Description/web-reference.xsd,web-reference.xsd
else
LIB_REFS = System System.EnterpriseServices System.Xml System.Data
LIB_MCS_FLAGS = -nowarn:168,169,219,414,612,649 -d:MONO_BROKEN_CONFIGURATION_DLL

ifndef NO_SYSTEM_WEB_DEPENDENCY
plainweb = $(the_libdir_base)plainweb/System.Web.dll
system_web_deps = $(plainweb)
LIB_REFS += plainweb/System.Web
endif

ifndef NO_SYSTEM_DESIGN_DEPENDENCY
plaindesign = $(the_libdir_base)plaindesign/System.Design.dll
system_design_deps = $(plaindesign)
LIB_REFS += plaindesign/System.Design
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

EXTRA_DISTFILES = \
	System.Web.Services.Description/web-reference.xsd	\
	System.Web.Services.Description/wsdl-1.1.xsd		\
	System.Web.Services.Description/wsdl-1.1-soap.xsd	\
	System.Web.Services.Description/wsdl.genxs		\
	Test/System.Web.Services.Description/*.wsdl

include ../../build/library.make

$(the_libdir_base)$(LIBRARY): $(system_web_deps) $(system_design_deps)

$(plainweb):
	(cd ../System.Web; $(MAKE) $@)

$(plaindesign):
	(cd ../System.Design; $(MAKE) $@)

.NOTPARALLEL: $(plainweb) $(plaindesign)
