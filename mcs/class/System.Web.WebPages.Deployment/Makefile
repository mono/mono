thisdir = class/System.Web.WebPages.Deployment
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.WebPages.Deployment.dll
LIBRARY_NAME = System.Web.WebPages.Deployment.dll

UPSTREAM_DIR = ../../../external/aspnetwebstack/src
RESX_DIST = \
	$(UPSTREAM_DIR)/System.Web.WebPages.Deployment/Resources/ConfigurationResources.resx \
	$(UPSTREAM_DIR)/CommonResources.resx

RESOURCES = $(subst $(UPSTREAM_DIR),$(build_libdir),$(RESX_DIST:.resx=.resources))

LIB_MCS_FLAGS = \
		/warn:1 \
		/noconfig \
		/keyfile:../winfx.pub \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /r:System.Configuration.dll \
	        /r:System.Web.dll \
	        /r:Microsoft.Web.Infrastructure.dll \
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

$(build_libdir)/System.Web.WebPages.Deployment/Resources/ConfigurationResources.resources: $(UPSTREAM_DIR)/System.Web.WebPages.Deployment/Resources/ConfigurationResources.resx
#	Doesn't work for some reason
#	$(run-resgen)
	mkdir -p $(dir `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`)
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)` `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

$(build_libdir)/CommonResources.resources: $(UPSTREAM_DIR)/CommonResources.resx
#	Doesn't work for some reason
#	$(run-resgen)
	mkdir -p $(dir `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`)
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)` `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
