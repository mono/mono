thisdir = class/System.Core
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Core.dll

ifeq ($(PROFILE),build)
CSC_RUNTIME_FLAGS=--profile=aot:output=$(topdir)/class/lib/$(PROFILE)/csc.$(LIBRARY).aotprofile
endif

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,PFX_LEGACY_3_5,FEATURE_NETCORE
LIB_REFS = System
LIB_MCS_FLAGS = $(REFERENCE_SOURCES_FLAGS) -d:INSIDE_SYSCORE -d:LIBC -unsafe -nowarn:436
TXT_RESOURCE_STRINGS = ../referencesource/System.Core/System.Core.txt
RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Linq.Expressions/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq.Parallel/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Linq.Queryable/src/Resources/Strings.resx

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_5
endif

ifdef AOT_FRIENDLY_PROFILE
extra_test_flags := -exclude:NotWorkingInterpreter
LIB_MCS_FLAGS += -d:NO_FEATURE_STATIC_DELEGATE,FEATURE_MAKE_RUN_METHODS
else
LIB_MCS_FLAGS += -d:FEATURE_COMPILE,FEATURE_COMPILE_TO_METHODBUILDER,FEATURE_PDB_GENERATOR
endif

ifeq (monotouch_runtime, $(PROFILE))
LIB_MCS_FLAGS += -d:FULL_AOT_RUNTIME
endif

ifneq (basic, $(PROFILE))
ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += Mono.Posix
endif
endif

CC_PROFILE := $(filter monotouch% xammac, $(PROFILE))
ifdef CC_PROFILE
BUILT_SOURCES = \
	../corlib/CommonCrypto/AesManaged.g.cs \
	../corlib/CommonCrypto/AesCryptoServiceProvider.g.cs

../corlib/CommonCrypto/%.g.cs:
	$(MAKE) -C ../corlib/CommonCrypto
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make

TEST_HARNESS_EXCLUDES += $(extra_test_flags)
