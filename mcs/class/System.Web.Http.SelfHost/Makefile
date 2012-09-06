thisdir = class/System.Web.Http.SelfHost
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Http.SelfHost.dll

System.Web.Http.SelfHost.Properties.CommonWebApiResources.resources: ../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.Http.SelfHost.Properties.SRResources.resources: ../../../external/aspnetwebstack/src/System.Web.Http.SelfHost/Properties/SRResources.resx
	$(RESGEN) "$<" "$@"


LIB_MCS_FLAGS = -r:System.Core.dll -r:System.dll -r:System.Xml.dll -r:System.Configuration.dll -r:System.Net.Http.dll \
		-r:System.Runtime.Serialization.dll -r:System.ServiceModel.dll -r:System.IdentityModel.dll \
		-r:System.Web.Http.dll -r:System.Net.Http.Formatting.dll \
		-d:ASPNETMVC -keyfile:../winfx.pub -delaysign

include ../../build/library.make

$(the_lib): System.Web.Http.SelfHost.Properties.CommonWebApiResources.resources \
	System.Web.Http.SelfHost.Properties.SRResources.resources