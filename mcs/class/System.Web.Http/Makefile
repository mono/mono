thisdir = class/System.Web.Http
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Http.dll

System.Web.Http.Properties.CommonWebApiResources.resources: ../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.Http.Properties.SRResources.resources: ../../../external/aspnetwebstack/src/System.Web.Http/Properties/SRResources.resx
	$(RESGEN) "$<" "$@"


LIB_MCS_FLAGS = -r:System.Core.dll -r:System.dll -r:System.Xml.dll -r:System.Net.Http.dll -r:System.ComponentModel.DataAnnotations.dll \
		-r:System.Net.Http.Formatting.dll -r:System.Runtime.Caching.dll -r:System.Runtime.Serialization.dll -r:System.Data.Linq.dll \
		-d:ASPNETMVC -keyfile:../winfx.pub -delaysign \
		-resource:System.Web.Http.Properties.CommonWebApiResources.resources \
		-resource:System.Web.Http.Properties.SRResources.resources

include ../../build/library.make

$(the_lib): System.Web.Http.Properties.CommonWebApiResources.resources \
	System.Web.Http.Properties.SRResources.resources