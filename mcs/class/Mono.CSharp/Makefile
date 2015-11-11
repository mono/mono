thisdir = class/Mono.CSharp
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.CSharp.dll

LIB_REFS = System.Core System.Xml System
LIB_MCS_FLAGS =

ifdef MOBILE_STATIC
LIB_MCS_FLAGS += -d:IOS_REFLECTION
endif

TEST_MCS_FLAGS = -r:System.Core.dll

include ../../build/library.make

