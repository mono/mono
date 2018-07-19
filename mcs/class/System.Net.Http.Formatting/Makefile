thisdir = class/System.Net.Http.Formatting
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Net.Http.Formatting.dll

RESOURCE_DEFS = \
	System.Net.Http.Properties.CommonWebApiResources,../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx \
	System.Net.Http.Properties.Resources,../../../external/aspnetwebstack/src/System.Net.Http.Formatting/Properties/Resources.resx


LIB_REFS = System.Core System System.Net.Http System.Xml System.Runtime.Serialization System.Xml.Linq System.Data System.Configuration
LIB_MCS_FLAGS = -d:ASPNETMVC -keyfile:../winfx.pub -delaysign 

include ../../build/library.make
