thisdir = class/System.Web.WebPages
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.WebPages.dll
LIBRARY_NAME = System.Web.WebPages.dll

UPSTREAM_DIR = ../../../external/aspnetwebstack/src
RESX_DIST = \
	$(UPSTREAM_DIR)/System.Web.WebPages/Resources/WebPageResources.resx \
	$(UPSTREAM_DIR)/CommonResources.resx

RESOURCES = $(subst $(UPSTREAM_DIR),$(build_libdir),$(RESX_DIST:.resx=.resources))

LIB_MCS_FLAGS = \
		/warn:1 \
		/noconfig \
		/keyfile:../winfx.pub \
		/delaysign \
		/r:Microsoft.CSharp.dll \
		/r:Microsoft.Web.Infrastructure.dll \
	        /r:System.dll \
	        /r:System.ComponentModel.DataAnnotations.dll \
	        /r:System.Configuration.dll \
	        /r:System.Core.dll \
	        /r:System.Data.Linq.dll \
	        /r:System.Web.dll \
	        /r:System.Web.WebPages.Deployment.dll \
	        /r:System.Web.Razor.dll \
	        /r:System.Xml.dll \
	        /r:System.Xml.Linq.dll \
	        /d:ASPNETWEBPAGES \
		$(RESOURCES:%=/resource:%)

EXTRA_DISTFILES = $(RESX_DIST)

include ../../build/library.make

$(build_lib): $(RESOURCES)

# Canned recipe which would be useful, but make doesn't run it for some reason...
#define run-resgen = 
#	mkdir -p $(dir `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`)
#	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)` `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
#endef

$(build_libdir)/System.Web.WebPages/Resources/WebPageResources.resources: $(UPSTREAM_DIR)/System.Web.WebPages/Resources/WebPageResources.resx
#	Doesn't work for some reason
#	$(run-resgen)
	mkdir -p $(dir `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`)
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)` `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

$(build_libdir)/CommonResources.resources: $(UPSTREAM_DIR)/CommonResources.resx
#	Doesn't work for some reason
#	$(run-resgen)
	mkdir -p $(dir `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`)
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)` `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
