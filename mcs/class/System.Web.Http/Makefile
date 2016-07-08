thisdir = class/System.Web.Http
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Http.dll

RESOURCE_DEFS = \
	System.Web.Http.Properties.CommonWebApiResources,../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx \
	System.Web.Http.Properties.SRResources,../../../external/aspnetwebstack/src/System.Web.Http/Properties/SRResources.resx



LIB_REFS = System.Core System System.Xml System.Net.Http System.ComponentModel.DataAnnotations System.Net.Http.Formatting System.Runtime.Caching System.Runtime.Serialization System.Data.Linq
LIB_MCS_FLAGS = -d:ASPNETMVC -keyfile:../winfx.pub -delaysign 

include ../../build/library.make
