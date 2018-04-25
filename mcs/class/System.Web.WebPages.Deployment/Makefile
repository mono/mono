thisdir = class/System.Web.WebPages.Deployment
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.WebPages.Deployment.dll
LIBRARY_NAME = System.Web.WebPages.Deployment.dll

RESOURCE_DEFS = \
	System.Web.WebPages.Deployment.Common.CommonResources,../../../external/aspnetwebstack/src/CommonResources.resx \
	System.Web.WebPages.Deployment.Resources.ConfigurationResources,../../../external/aspnetwebstack/src/System.Web.WebPages.Deployment/Resources/ConfigurationResources.resx


LIB_REFS = System System.Core System.Configuration System.Web Microsoft.Web.Infrastructure
KEYFILE=../winfx.pub
LIB_MCS_FLAGS = \
			/warn:1 \
			-delaysign \
			/d:ASPNETWEBPAGES 

include ../../build/library.make
