thisdir = class/System.Core
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Core.dll

LIB_MCS_FLAGS = -d:INSIDE_SYSCORE -d:LIBC /r:System.dll -unsafe

INTERPRETER_DEP := Mono.Dynamic.Interpreter.dll
INTERPRETER_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(INTERPRETER_DEP))

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_5 -nowarn:1720
endif

ifeq (monodroid, $(PROFILE))
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR,FEATURE_REFEMIT,ANDROID
endif

ifeq (xammac, $(PROFILE))
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR,FEATURE_REFEMIT
endif

ifeq (monotouch, $(PROFILE))
LIBRARY_USE_INTERMEDIATE_FILE = yes

CYCLIC_DEPS := $(INTERPRETER_DEP)
CYCLIC_DEP_FILES := $(INTERPRETER_DEP_FILE)
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR

ifdef CYCLIC_DEP_FILES
LIB_MCS_FLAGS += -d:MONO_INTERPRETER -r:$(INTERPRETER_DEP)
else
NO_SIGN_ASSEMBLY = yes
NO_INSTALL = yes
endif

endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -d:FEATURE_CORE_DLR,FEATURE_REFEMIT,FEATURE_PDBEMIT
endif

ifneq (basic, $(PROFILE))
CLR_PROFILE := $(filter 2.0 4.0 4.5, $(FRAMEWORK_VERSION))
endif

ifdef CLR_PROFILE
LIB_MCS_FLAGS += -r:Mono.Posix.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make

ifdef CYCLIC_DEP_FILES
TEST_HARNESS_EXCLUDES += -exclude:NotWorkingInterpreter
$(build_lib): $(INTERPRETER_DEP_FILE)
endif

