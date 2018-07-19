thisdir = class/Mono.Reactive.Testing
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Reactive.Testing.dll
LIB_REFS = System System.Core System.Reactive.Interfaces System.Reactive.Core System.Reactive.Linq System.Reactive.PlatformServices System.Reactive.Providers System.Reactive.Runtime.Remoting System.Reactive.Experimental System.Reactive.Windows.Forms System.Reactive.Windows.Threading System.Reactive.Observable.Aliases System.Windows.Forms WindowsBase nunitlite
LIB_MCS_FLAGS = \
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

EXTRA_DISTFILES =

VALID_PROFILE := $(filter net_4_x, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Mono.Reactive.Testing.dll
NO_TEST = yes
endif

NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes

include ../../build/library.make
