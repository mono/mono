thisdir = class/System.Web.Http
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Http.dll

System.Web.Http.Properties.CommonWebApiResources.resources: ../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.Http.Properties.SRResources.resources: ../../../external/aspnetwebstack/src/System.Web.Http/Properties/SRResources.resx
	$(RESGEN) "$<" "$@"


LIB_REFS = System.Core System System.Xml System.Net.Http System.ComponentModel.DataAnnotations System.Net.Http.Formatting System.Runtime.Caching System.Runtime.Serialization System.Data.Linq
LIB_MCS_FLAGS = \
		-d:ASPNETMVC -keyfile:../winfx.pub -delaysign \
		-resource:System.Web.Http.Properties.CommonWebApiResources.resources \
		-resource:System.Web.Http.Properties.SRResources.resources

include ../../build/library.make

$(the_lib): System.Web.Http.Properties.CommonWebApiResources.resources \
	System.Web.Http.Properties.SRResources.resources
