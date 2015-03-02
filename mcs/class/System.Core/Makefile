thisdir = class/System.Core
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Core.dll

REFERENCE_SOURCES_FLAGS = -d:FEATURE_PAL,PFX_LEGACY_3_5,FEATURE_NETCORE
LIB_REFS = System
LIB_MCS_FLAGS = $(REFERENCE_SOURCES_FLAGS) -d:INSIDE_SYSCORE -d:LIBC -unsafe
RESOURCE_STRINGS = ../../../external/referencesource/System.Core/System.Core.txt

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_5 -nowarn:1720
endif

MOBILE_DYNAMIC := $(filter monodroid xammac mobile, $(PROFILE))
MOBILE_STATIC := $(filter mobile_static monotouch monotouch_runtime, $(PROFILE))

ifdef MOBILE_DYNAMIC
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR,FEATURE_REFEMIT
endif

system_core_plain_libdir = $(the_libdir_base)plaincore

ifdef MOBILE_STATIC
extra_test_flags := -exclude:NotWorkingInterpreter
system_core_library_deps = $(the_libdir_base)Mono.Dynamic.Interpreter.dll $(system_core_plain_libdir)/System.Core.dll
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR

ifneq (plaincore/,$(intermediate))
LIB_REFS += Mono.Dynamic.Interpreter
LIB_MCS_FLAGS += -d:MONO_INTERPRETER
endif

endif

ifeq (monotouch_runtime, $(PROFILE))
LIB_MCS_FLAGS += -d:FULL_AOT_RUNTIME
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR,FEATURE_REFEMIT,FEATURE_PDBEMIT
endif

ifneq (basic, $(PROFILE))
CLR_PROFILE := $(filter 2.0 4.0 4.5, $(FRAMEWORK_VERSION))
endif

ifdef CLR_PROFILE
LIB_REFS += Mono.Posix
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make

$(the_libdir_base)System.Core.dll: $(system_core_library_deps)

.NOTPARALLEL: $(system_core_plain_libdir)/System.Core.dll $(the_libdir_base)Mono.Dynamic.Interpreter.dll

ifneq (plaincore/,$(intermediate))
$(system_core_plain_libdir)/System.Core.dll:
	$(MAKE) intermediate=plaincore/ $(system_core_plain_libdir)/System.Core.dll
endif

$(the_libdir_base)Mono.Dynamic.Interpreter.dll:
	(cd ../Mono.Dynamic.Interpreter; $(MAKE))

TEST_HARNESS_EXCLUDES += $(extra_test_flags)

CLEAN_FILES = $(system_core_plain_libdir)/System.Core.dll

ifndef intermediate
ifneq ($(PROFILE),basic)
csproj-local:
	$(MAKE) csproj-local intermediate=plaincore/
endif
endif
