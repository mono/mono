thisdir = class/System.Web.WebPages
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.WebPages.dll
LIBRARY_NAME = System.Web.WebPages.dll

System.Web.WebPages.Common.CommonResources.resources: ../../../external/aspnetwebstack/src/CommonResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.WebPages.Resources.WebPageResources.resources: ../../../external/aspnetwebstack/src/System.Web.WebPages/Resources/WebPageResources.resx
	$(RESGEN) "$<" "$@"

LIB_MCS_FLAGS = \
		/warn:1 \
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
		/resource:System.Web.WebPages.Resources.WebPageResources.resources \
		/resource:System.Web.WebPages.Common.CommonResources.resources

EXTRA_DISTFILES = $(RESX_DIST)

include ../../build/library.make

$(build_lib): System.Web.WebPages.Resources.WebPageResources.resources \
		System.Web.WebPages.Common.CommonResources.resources
