thisdir = class/System.Web.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Services.dll
MOBILE := $(filter monotouch monodroid xammac mobile mobile_static, $(PROFILE))
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
LIB_REFS = System System.EnterpriseServices System.Xml System.Web
LIB_MCS_FLAGS = \
	-nowarn:649 -nowarn:169			\
	-r:$(corlib)				\
	-r:System.dll				\
	-r:System.EnterpriseServices.dll	\
	-r:System.Xml.dll			\
	-r:System.Web.dll

plainweb_dir = $(the_libdir_base)plainweb
LOCAL_MCS_FLAGS = -lib:$(plainweb_dir)

plainweb = $(plainweb_dir)/System.Web.dll
system_web_services_deps = $(plainweb)

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

$(the_libdir_base)$(LIBRARY): $(system_web_services_deps)

$(plainweb):
	(cd ../System.Web; $(MAKE) $@)

.NOTPARALLEL: $(plainweb)
