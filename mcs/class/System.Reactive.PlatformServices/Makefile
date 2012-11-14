thisdir = class/System.Reactive.PlatformServices
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Reactive.PlatformServices.dll
LIB_MCS_FLAGS = \
		@more_build_args \
	        -r:System.dll \
	        -r:System.Core.dll \
		-r:System.Reactive.Interfaces.dll \
		-r:System.Reactive.Core.dll \
		-r:System.Reactive.Linq.dll

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NO_TASK_DELAY
endif

NET_4_5 := $(filter 4.5, $(FRAMEWORK_VERSION))
ifdef NET_4_5
LIB_MCS_FLAGS += -d:HAS_EDI -d:PREFERASYNC -d:PREFER_ASYNC
endif

NO_TASK_DELAY := $(filter 4.5 2.1, $(FRAMEWORK_VERSION))
ifndef NO_TASK_DELAY
LIB_MCS_FLAGS += -d:NO_TASK_DELAY
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -r:Mono.Reactive.Tests.dll

EXTRA_DISTFILES = more_build_args

VALID_PROFILE := $(filter 2 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.System.Reactive.PlatformServices.dll
NO_INSTALL = yes
endif

NO_SIGN_ASSEMBLY = yes
NO_TEST = yes

include ../../build/library.make
