thisdir = class/System.Web.Mvc3
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Mvc3.dll
LIBRARY_NAME = System.Web.Mvc.dll

RESOURCE_DEFS = System.Web.Mvc.Resources.MvcResources,Mvc/Resources/MvcResources.resx
RESX_DIST =  Mvc/Resources/MvcResources.resx

LIB_REFS = Microsoft.Web.Infrastructure System System.Core System.Configuration System.Data System.Xml System.Web System.Web.Abstractions System.Web.Routing System.Web.Extensions System.ComponentModel.DataAnnotations System.Data.Linq System.Runtime.Caching System.Web.Razor System.Web.WebPages.Razor System.Web.WebPages
KEYFILE = ../winfx.pub
LIB_MCS_FLAGS = \
		/warn:1 \
		/d:MONO \
		/delaysign

EXTRA_DISTFILES = $(RESX_DIST)

include ../../build/library.make

