thisdir = class/System.Web.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Services.dll
MOBILE := $(filter monotouch monotouch_watch monodroid xammac mobile mobile_static, $(PROFILE))
ifdef MOBILE
LIB_REFS = System System.Xml
LIB_MCS_FLAGS = \
	-nowarn:649 -nowarn:169			\
	-r:$(corlib)				\
	-r:System.dll				\
	-r:System.Xml.dll			\
	-resource:System.Web.Services.Description/wsdl-1.1.xsd,wsdl-1.1.xsd \
	-resource:System.Web.Services.Description/wsdl-1.1-soap.xsd,wsdl-1.1-soap.xsd \
	-resource:System.Web.Services.Description/web-reference.xsd,web-reference.xsd
else
LIB_REFS = System System.EnterpriseServices System.Xml System.Data
LIB_MCS_FLAGS = -nowarn:168,169,219,414,612,649 -d:MONO_BROKEN_CONFIGURATION_DLL

ifndef NO_SYSTEM_WEB_DEPENDENCY
LIB_REFS += System.Web
plainweb_dir = $(the_libdir_base)plainweb
plainweb = $(plainweb_dir)/System.Web.dll
system_web_services_deps = $(plainweb)
LOCAL_MCS_FLAGS += -lib:$(plainweb_dir)
endif

ifndef NO_SYSTEM_DESIGN_DEPENDENCY
LIB_REFS += System.Design
plaindesign_dir = $(the_libdir_base)plaindesign
plaindesign = $(plaindesign_dir)/System.Design.dll
system_design_deps = $(plaindesign)
LOCAL_MCS_FLAGS += -lib:$(plaindesign_dir)
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

EXTRA_DISTFILES = \
	System.Web.Services.Description/web-reference.xsd	\
	System.Web.Services.Description/wsdl-1.1.xsd		\
	System.Web.Services.Description/wsdl-1.1-soap.xsd	\
	System.Web.Services.Description/wsdl.genxs		\
	Test/System.Web.Services.Description/*.wsdl

include ../../build/library.make

$(the_libdir_base)$(LIBRARY): $(system_web_services_deps) $(system_design_deps)

$(plainweb):
	(cd ../System.Web; $(MAKE) $@)

$(plaindesign):
	(cd ../System.Design; $(MAKE) $@)

.NOTPARALLEL: $(plainweb) $(plaindesign)
