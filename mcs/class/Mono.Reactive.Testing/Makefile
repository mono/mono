thisdir = class/Mono.Reactive.Testing
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Reactive.Testing.dll
LIB_MCS_FLAGS = \
		@more_build_args \
	        -r:System.dll \
	        -r:System.Core.dll \
		-r:System.Reactive.Interfaces.dll \
		-r:System.Reactive.Core.dll \
		-r:System.Reactive.Linq.dll \
		-r:System.Reactive.PlatformServices.dll \
		-r:System.Reactive.Providers.dll \
		-r:System.Reactive.Runtime.Remoting.dll \
		-r:System.Reactive.Experimental.dll \
		-r:System.Reactive.Windows.Forms.dll \
		-r:System.Reactive.Windows.Threading.dll \
		-r:System.Reactive.Observable.Aliases.dll \
		-r:System.Windows.Forms.dll \
		-r:WindowsBase.dll \
		-r:nunit.framework.dll \
		-d:NUNIT -d:MONO -d:DESKTOPCLR
		# NO_PERF is required to disable ObservableMultipleTest.Catch_TailRecursive2 which blocked test execution.
		# disabled -d:HAS_WINFORMS

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NO_TASK_DELAY
endif

NET_4_5 := $(filter 4.5, $(FRAMEWORK_VERSION))
ifdef NET_4_5
LIB_MCS_FLAGS += -d:HAS_EDI -d:PREFERASYNC -d:PREFER_ASYNC
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618

EXTRA_DISTFILES = more_build_args

VALID_PROFILE := $(filter net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Mono.Reactive.Testing.dll
NO_TEST = yes
endif

NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes

include ../../build/library.make
