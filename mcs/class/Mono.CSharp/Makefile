thisdir = class/Mono.CSharp
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.CSharp.dll

LIB_MCS_FLAGS = -r:System.Core.dll -r:System.Xml.dll -r:System.dll

MOBILE_STATIC := $(filter mobile_static monotouch, $(PROFILE))

ifdef MOBILE_STATIC
LIB_MCS_FLAGS += -d:IOS_REFLECTION
endif

TEST_MCS_FLAGS = -r:System.Core.dll

include ../../build/library.make

