thisdir = class/System.Net.Http.Formatting
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Net.Http.Formatting.dll

System.Net.Http.Properties.CommonWebApiResources.resources: ../../../external/aspnetwebstack/src/Common/CommonWebApiResources.resx
	$(RESGEN) "$<" "$@"

System.Net.Http.Properties.Resources.resources: ../../../external/aspnetwebstack/src/System.Net.Http.Formatting/Properties/Resources.resx
	$(RESGEN) "$<" "$@"

LIB_REFS = System.Core System System.Net.Http System.Xml System.Runtime.Serialization System.Xml.Linq System.Data System.Configuration
LIB_MCS_FLAGS = \
		-d:ASPNETMVC -keyfile:../winfx.pub -delaysign \
		-resource:System.Net.Http.Properties.CommonWebApiResources.resources \
		-resource:System.Net.Http.Properties.Resources.resources

include ../../build/library.make

$(the_lib): System.Net.Http.Properties.CommonWebApiResources.resources \
	System.Net.Http.Properties.Resources.resources
