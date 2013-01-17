thisdir = class/System.Web.Mvc2
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Mvc2.dll
LIBRARY_NAME = System.Web.Mvc.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

RESX_DIST =  System.Web.Mvc/Resources/MvcResources.resx

LIB_MCS_FLAGS = \
		/warn:1 \
		/keyfile:../winfx.pub \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /r:System.Configuration.dll \
	        /r:System.Data.dll \
	        /r:System.Xml.dll \
	        /r:System.Web.dll \
	        /r:System.Web.Abstractions.dll \
	        /r:System.Web.Routing.dll \
	        /r:System.Web.Extensions.dll \
		/r:System.ComponentModel.DataAnnotations.dll \
		/r:System.Data.Linq.dll \
		$(foreach r, $(RESOURCES), /resource:$(r),System.Web.Mvc.Resources.$(notdir $(r)))

ifeq (2.0, $(FRAMEWORK_VERSION))
# This is a .NET 3.5 only assembly, but built during the 2.0 build
LIB_MCS_FLAGS += -d:NET_3_5 -d:MONO
endif

EXTRA_DISTFILES = $(RESX_DIST)

# This is a .NET 3.5+ assembly - it must be built ONLY in the 2.0 profile
VALID_PROFILE := $(filter net_2_0, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Web.Mvc.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
else
RESOURCES = $(RESX_DIST:.resx=.resources)
endif

include ../../build/library.make

$(build_lib): $(RESOURCES)

$(RESOURCES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
