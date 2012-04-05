thisdir = class/System.Web.Razor
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Razor.dll
LIBRARY_NAME = System.Web.Razor.dll

System.Web.Razor.Common.CommonResources.resources: ../../../external/aspnetwebstack/src/CommonResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.Razor.Resources.RazorResources.resources: ../../../external/aspnetwebstack/src/System.Web.Razor/Resources/RazorResources.resx
	$(RESGEN) "$<" "$@"

LIB_MCS_FLAGS = \
		/warn:1 \
		/keyfile:../winfx.pub -delaysign \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /d:ASPNETWEBPAGES \
		/resource:System.Web.Razor.Resources.RazorResources.resources \
		/resource:System.Web.Razor.Common.CommonResources.resources

include ../../build/library.make

$(build_lib): System.Web.Razor.Resources.RazorResources.resources \
		System.Web.Razor.Common.CommonResources.resources
