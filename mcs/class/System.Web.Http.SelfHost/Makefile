thisdir = class/System.Web.Http.SelfHost
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Http.SelfHost.dll

System.Web.Http.SelfHost.Properties.CommonWebApiResources.resources: ../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx
	$(RESGEN) "$<" "$@"

System.Web.Http.SelfHost.Properties.SRResources.resources: ../../../external/aspnetwebstack/src/System.Web.Http.SelfHost/Properties/SRResources.resx
	$(RESGEN) "$<" "$@"


LIB_REFS = System.Core System System.Xml System.Configuration System.Net.Http System.Runtime.Serialization System.ServiceModel System.IdentityModel System.Web.Http System.Net.Http.Formatting
LIB_MCS_FLAGS = -d:ASPNETMVC -keyfile:../winfx.pub -delaysign

include ../../build/library.make

$(the_lib): System.Web.Http.SelfHost.Properties.CommonWebApiResources.resources \
	System.Web.Http.SelfHost.Properties.SRResources.resources
