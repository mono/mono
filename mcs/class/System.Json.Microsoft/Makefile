thisdir = class/System.Json.Microsoft
SUBDIRS = 
include ../../build/rules.make

RESOURCE_DEFS = System.Json.Properties.Resources,System.Json/Properties/Resources.resx

LIBRARY = System.Json.Microsoft.dll
LIB_REFS = System System.Xml System.Core System.Runtime.Serialization
LIB_MCS_FLAGS = /d:ASPNETMVC -keyfile:../winfx.pub -delaysign 
EXTRA_DISTFILES = System.Json/Properties/Resources.resx

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += Microsoft.CSharp
LIB_MCS_FLAGS += -d:FEATURE_DYNAMIC
endif

ifdef MOBILE_DYNAMIC
LIB_MCS_FLAGS += -d:FEATURE_DYNAMIC
endif

TEST_LIB_REFS = System.Core

include ../../build/library.make
