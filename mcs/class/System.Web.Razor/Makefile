thisdir = class/System.Web.Razor
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Razor.dll
LIBRARY_NAME = System.Web.Razor.dll

RESX_DIST = \
	../../../external/aspnetwebstack/src/System.Web.Razor/Resources/RazorResources.resx \
	../../../external/aspnetwebstack/src/CommonResources.resx

RESX_RES = $(RESX_DIST:.resx=.resources)

LIB_MCS_FLAGS = \
		/warn:1 \
		/noconfig \
		/keyfile:../winfx.pub \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /d:ASPNETWEBPAGES \
		$(RESX_RES:%=/resource:%)

EXTRA_DISTFILES = $(RESX_DIST)

RESOURCES = $(RESX_DIST:.resx=.resources)

include ../../build/library.make

$(build_lib): $(RESOURCES)

$(RESOURCES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
