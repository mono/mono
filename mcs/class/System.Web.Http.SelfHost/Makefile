thisdir = class/System.Web.Http.SelfHost
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Http.SelfHost.dll

RESOURCE_DEFS = \
	System.Web.Http.SelfHost.Properties.CommonWebApiResources,../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx \
	System.Web.Http.SelfHost.Properties.SRResources,../../../external/aspnetwebstack/src/System.Web.Http.SelfHost/Properties/SRResources.resx

LIB_REFS = System.Core System System.Xml System.Configuration System.Net.Http System.Runtime.Serialization System.ServiceModel System.IdentityModel System.Web.Http System.Net.Http.Formatting
LIB_MCS_FLAGS = -d:ASPNETMVC -keyfile:../winfx.pub -delaysign

include ../../build/library.make

