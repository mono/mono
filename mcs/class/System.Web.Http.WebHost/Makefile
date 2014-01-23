thisdir = class/System.Web.Http.WebHost	
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Http.WebHost.dll

System.Web.Http.WebHost.Properties.CommonWebApiResources.resources: ../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.Http.WebHost.Properties.SRResources.resources: ../../../external/aspnetwebstack/src/System.Web.Http.WebHost/Properties/SRResources.resx
	$(RESGEN) "$<" "$@"


LIB_MCS_FLAGS = -r:System.Core.dll -r:System.dll -r:System.Xml.dll -r:System.Configuration.dll -r:System.Net.Http.dll \
		-r:System.Runtime.Serialization.dll -r:System.ServiceModel.dll -r:System.IdentityModel.dll \
		-r:System.Web.Http.dll -r:System.Net.Http.Formatting.dll  -r:System.Web.Routing.dll -r:System.Web.dll \
        -r:Microsoft.Web.Infrastructure.dll \
		-d:ASPNETMVC -keyfile:../winfx.pub -delaysign

include ../../build/library.make

$(the_lib): System.Web.Http.WebHost.Properties.CommonWebApiResources.resources \
	System.Web.Http.WebHost.Properties.SRResources.resources
