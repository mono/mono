thisdir = class/System.Web.Razor
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Razor.dll
LIBRARY_NAME = System.Web.Razor.dll

RESOURCE_DEFS = \
	System.Web.Razor.Common.CommonResources,../../../external/aspnetwebstack/src/CommonResources.resx \
	System.Web.Razor.Resources.RazorResources,../../../external/aspnetwebstack/src/System.Web.Razor/Resources/RazorResources.resx

LIB_REFS = System System.Core
KEYFILE = ../winfx.pub
LIB_MCS_FLAGS = \
		/warn:1 \
		-delaysign \
	    /d:ASPNETWEBPAGES 

include ../../build/library.make
