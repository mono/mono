thisdir = class/System.Web.WebPages
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.WebPages.dll
LIBRARY_NAME = System.Web.WebPages.dll

RESOURCE_DEFS = \
	System.Web.WebPages.Common.CommonResources,../../../external/aspnetwebstack/src/CommonResources.resx \
	System.Web.WebPages.Resources.WebPageResources,../../../external/aspnetwebstack/src/System.Web.WebPages/Resources/WebPageResources.resx

LIB_REFS = Microsoft.CSharp Microsoft.Web.Infrastructure System System.ComponentModel.DataAnnotations System.Configuration System.Core System.Data.Linq System.Web System.Web.WebPages.Deployment System.Web.Razor System.Xml System.Xml.Linq

KEYFILE=../winfx.pub
LIB_MCS_FLAGS = \
			/warn:1 \
			/delaysign \
	        /d:ASPNETWEBPAGES 

EXTRA_DISTFILES = $(RESX_DIST)

include ../../build/library.make
