thisdir = class/System.Web.Mvc3
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Mvc3.dll
LIBRARY_NAME = System.Web.Mvc.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

RESX_DIST =  Mvc/Resources/MvcResources.resx

LIB_MCS_FLAGS = \
		/warn:1 \
		/keyfile:../winfx.pub \
		/d:MONO \
		/delaysign \
		/r:Microsoft.Web.Infrastructure.dll \
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
		/r:System.Runtime.Caching.dll \
		/r:System.Web.Razor.dll \
		/r:System.Web.WebPages.Razor.dll \
		/r:System.Web.WebPages.dll \
		$(foreach r, $(RESOURCES), /resource:$(r),System.Web.Mvc.Resources.$(notdir $(r)))

EXTRA_DISTFILES = $(RESX_DIST)
RESOURCES = $(RESX_DIST:.resx=.resources)
include ../../build/library.make

$(build_lib): $(RESOURCES)

$(RESOURCES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
