thisdir = class/System.Web.Http.WebHost
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Http.WebHost.dll

RESOURCE_DEFS = \
	System.Web.Http.WebHost.Properties.CommonWebApiResources,../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx \
	System.Web.Http.WebHost.Properties.SRResources,../../../external/aspnetwebstack/src/System.Web.Http.WebHost/Properties/SRResources.resx

LIB_REFS = System.Core System System.Xml System.Configuration System.Net.Http System.Runtime.Serialization System.ServiceModel System.IdentityModel System.Web.Http System.Net.Http.Formatting System.Web.Routing System.Web Microsoft.Web.Infrastructure
LIB_MCS_FLAGS = -d:ASPNETMVC -keyfile:../winfx.pub -delaysign

include ../../build/library.make

$(the_lib): 
