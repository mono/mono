thisdir = class/System.Json.Microsoft
SUBDIRS = 
include ../../build/rules.make

System.Json.Properties.Resources.resources: System.Json/Properties/Resources.resx
	$(RESGEN) "$<" "$@"

LIBRARY = System.Json.Microsoft.dll
LIB_REFS = System System.Xml System.Core System.Runtime.Serialization
LIB_MCS_FLAGS = /d:ASPNETMVC -keyfile:../winfx.pub -delaysign \
		/resource:System.Json.Properties.Resources.resources

EXTRA_DISTFILES = System.Json/Properties/Resources.resx

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += Microsoft.CSharp
LIB_MCS_FLAGS += -d:FEATURE_DYNAMIC
endif

ifeq (monodroid, $(PROFILE))
LIB_MCS_FLAGS += -d:FEATURE_DYNAMIC
endif

ifeq (xammac, $(PROFILE))
LIB_MCS_FLAGS += -d:FEATURE_DYNAMIC
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make

$(the_lib): System.Json.Properties.Resources.resources
