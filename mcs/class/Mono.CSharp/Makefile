thisdir = class/Mono.CSharp
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.CSharp.dll

LIB_REFS = System.Core System.Xml System
LIB_MCS_FLAGS =

ifdef MOBILE_STATIC
LIB_MCS_FLAGS += -d:IOS_REFLECTION
endif

ifndef NO_THREAD_ABORT
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
endif

TEST_MCS_FLAGS =
TEST_LIB_REFS = System.Core

LIB_MCS_FLAGS += $(REFERENCE_SOURCES_FLAGS)

include ../../build/library.make

$(topdir)/mcs/cs-parser.cs:
	$(MAKE) -C $(topdir)/mcs cs-parser.cs

$(the_lib): $(topdir)/mcs/cs-parser.cs
