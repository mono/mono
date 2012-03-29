thisdir = class/System.Web.Razor
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Razor.dll
LIBRARY_NAME = System.Web.Razor.dll

UPSTREAM_DIR = ../../../external/aspnetwebstack/src
RESX_DIST = \
	$(UPSTREAM_DIR)/System.Web.Razor/Resources/RazorResources.resx \
	$(UPSTREAM_DIR)/CommonResources.resx

RESOURCES = $(subst $(UPSTREAM_DIR),$(build_libdir),$(RESX_DIST:.resx=.resources))

LIB_MCS_FLAGS = \
		/warn:1 \
		/noconfig \
		/keyfile:../winfx.pub \
	        /r:System.dll \
	        /r:System.Core.dll \
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

$(build_libdir)/System.Web.Razor/Resources/RazorResources.resources: $(UPSTREAM_DIR)/System.Web.Razor/Resources/RazorResources.resx
#	Doesn't work for some reason
#	$(run-resgen)
	mkdir -p $(dir `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`)
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)` `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

$(build_libdir)/CommonResources.resources: $(UPSTREAM_DIR)/CommonResources.resx
#	Doesn't work for some reason
#	$(run-resgen)
	mkdir -p $(dir `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`)
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)` `echo $@ | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
