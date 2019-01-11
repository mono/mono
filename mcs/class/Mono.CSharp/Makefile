thisdir = class/Mono.CSharp
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.CSharp.dll

LIB_REFS = System.Core System.Xml System
KEYFILE = ../mono.pub
LIB_MCS_FLAGS =

ifdef AOT_FRIENDLY_PROFILE
LIB_MCS_FLAGS += -d:IOS_REFLECTION
endif

ifndef NO_THREAD_ABORT
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_THREAD_ABORT
endif

TEST_MCS_FLAGS =
TEST_LIB_REFS = System.Core

ifndef NO_PROCESS_START
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_PROCESS_START
TEST_MCS_FLAGS += -d:MONO_FEATURE_PROCESS_START
endif


LIB_MCS_FLAGS += $(REFERENCE_SOURCES_FLAGS)

BUILT_SOURCES = $(PROFILE)-parser.cs

$(PROFILE)-parser.cs: $(topdir)/mcs/cs-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -c -o $(PROFILE)-jay-tmp.out $< < $(topdir)/jay/skeleton.cs && mv $(PROFILE)-jay-tmp.out $@

include ../../build/library.make

