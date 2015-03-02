thisdir = class/System.Web.WebPages
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.WebPages.dll
LIBRARY_NAME = System.Web.WebPages.dll

System.Web.WebPages.Common.CommonResources.resources: ../../../external/aspnetwebstack/src/CommonResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.WebPages.Resources.WebPageResources.resources: ../../../external/aspnetwebstack/src/System.Web.WebPages/Resources/WebPageResources.resx
	$(RESGEN) "$<" "$@"

LIB_REFS = Microsoft.CSharp Microsoft.Web.Infrastructure System System.ComponentModel.DataAnnotations System.Configuration System.Core System.Data.Linq System.Web System.Web.WebPages.Deployment System.Web.Razor System.Xml System.Xml.Linq
LIB_MCS_FLAGS = \
		/warn:1 \
		/keyfile:../winfx.pub \
		/delaysign \
	        /d:ASPNETWEBPAGES \
		/resource:System.Web.WebPages.Resources.WebPageResources.resources \
		/resource:System.Web.WebPages.Common.CommonResources.resources

EXTRA_DISTFILES = $(RESX_DIST)

include ../../build/library.make

$(build_lib): System.Web.WebPages.Resources.WebPageResources.resources \
		System.Web.WebPages.Common.CommonResources.resources
