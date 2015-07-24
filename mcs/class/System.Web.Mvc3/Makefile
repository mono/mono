thisdir = class/System.Web.Mvc3
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Mvc3.dll
LIBRARY_NAME = System.Web.Mvc.dll

RESX_DIST =  Mvc/Resources/MvcResources.resx

LIB_REFS = Microsoft.Web.Infrastructure System System.Core System.Configuration System.Data System.Xml System.Web System.Web.Abstractions System.Web.Routing System.Web.Extensions System.ComponentModel.DataAnnotations System.Data.Linq System.Runtime.Caching System.Web.Razor System.Web.WebPages.Razor System.Web.WebPages
LIB_MCS_FLAGS = \
		/warn:1 \
		/keyfile:../winfx.pub \
		/d:MONO \
		/delaysign \
		$(foreach r, $(RESOURCES), /resource:$(r),System.Web.Mvc.Resources.$(notdir $(r)))

EXTRA_DISTFILES = $(RESX_DIST)
RESOURCES = $(RESX_DIST:.resx=.resources)
include ../../build/library.make

$(build_lib): $(RESOURCES)

$(RESOURCES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
