thisdir = class/System.Reactive.Windows.Forms
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Reactive.Windows.Forms.dll
LIB_MCS_FLAGS = \
		@more_build_args \
	        -r:System.dll \
	        -r:System.Core.dll \
		-r:System.Reactive.Interfaces.dll \
		-r:System.Reactive.Core.dll \
		-r:System.Reactive.Linq.dll \
		-r:System.Windows.Forms.dll

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NO_TASK_DELAY
endif

NET_4_5 := $(filter 4.5, $(FRAMEWORK_VERSION))
ifdef NET_4_5
LIB_MCS_FLAGS += -d:HAS_EDI -d:PREFERASYNC -d:PREFER_ASYNC
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = more_build_args

VALID_PROFILE := $(filter net_4_0 net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.System.Reactive.Windows.Forms.dll
NO_SIGN_ASSEMBLY = yes
endif

INSTALL_PROFILE := $(filter net_4_5, $(PROFILE))
ifndef INSTALL_PROFILE
NO_INSTALL = yes
endif

NO_TEST = yes

include ../../build/library.make
