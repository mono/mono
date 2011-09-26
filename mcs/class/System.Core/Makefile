thisdir = class/System.Core
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Core.dll

LIB_MCS_FLAGS = -d:INSIDE_SYSCORE -d:LIBC /r:$(corlib) /r:System.dll

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_5 -nowarn:1720
endif

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:CODEPLEX_40 -d:SILVERLIGHT
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -d:CODEPLEX_40
endif

ifneq (basic, $(PROFILE))
CLR_PROFILE := $(filter 2.0 4.0 4.5, $(FRAMEWORK_VERSION))
endif

ifdef CLR_PROFILE
LIB_MCS_FLAGS += -r:Mono.Posix.dll -unsafe
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

# This is a .NET 2.0+ only assembly
VALID_PROFILE := $(filter 2.0 2.1 4.0 4.5, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Core.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_BUILD = yes
endif

include ../../build/library.make

