thisdir = class/System.Web.WebPages.Razor
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.WebPages.Razor.dll
LIBRARY_NAME = System.Web.WebPages.Razor.dll

System.Web.WebPages.Razor.Common.CommonResources.resources: ../../../external/aspnetwebstack/src/CommonResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.WebPages.Razor.Resources.RazorWebResources.resources: ../../../external/aspnetwebstack/src/System.Web.WebPages.Razor/Resources/RazorWebResources.resx
	$(RESGEN) "$<" "$@"

LIB_MCS_FLAGS = \
		/warn:1 \
		/keyfile:../winfx.pub \
		/delaysign \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /r:System.Configuration.dll \
	        /r:System.Web.dll \
	        /r:System.Web.WebPages.dll \
	        /r:System.Web.Razor.dll \
	        /d:ASPNETWEBPAGES \
		/resource:System.Web.WebPages.Razor.Resources.RazorWebResources.resources \
		/resource:System.Web.WebPages.Razor.Common.CommonResources.resources

include ../../build/library.make

$(build_lib): System.Web.WebPages.Razor.Resources.RazorWebResources.resources \
		System.Web.WebPages.Razor.Common.CommonResources.resources


