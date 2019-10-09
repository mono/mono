thisdir = class/System.Net.Http

include ../../build/rules.make

LIBRARY = System.Net.Http.dll

LIB_REFS = System.Core System
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = $(EXTRA_LIB_MCS_FLAGS) -unsafe -nowarn:436
ifeq (monodroid,$(PROFILE))
LIB_MCS_FLAGS += -d:XAMARIN_MODERN
endif

ifeq (monotouch,$(PROFILE))
API_BIN_REFS := Xamarin.iOS
LIB_MCS_FLAGS += -d:XAMARIN_MODERN
endif

ifeq (monotouch_tv,$(PROFILE))
API_BIN_REFS := Xamarin.TVOS
LIB_MCS_FLAGS += -d:XAMARIN_MODERN
endif

ifeq (monotouch_watch,$(PROFILE))
API_BIN_REFS := Xamarin.WatchOS
LIB_MCS_FLAGS += -d:XAMARIN_MODERN
endif

ifeq (xammac,$(PROFILE))
API_BIN_REFS := Xamarin.Mac
LIB_MCS_FLAGS += -d:XAMARIN_MODERN
endif

ifeq (xammac_net_4_5,$(PROFILE))
# we can't use API_BIN_REFS for xammac_net_4_5 as it inherits from net_4_x.make
LIB_MCS_FLAGS += -r:$(topdir)/../external/binary-reference-assemblies/build/monotouch/Xamarin.Mac.dll
LIB_MCS_FLAGS += -d:XAMARIN_MODERN
endif

ifeq (wasm,$(PROFILE))
API_BIN_REFS := WebAssembly.Net.Http
endif

TEST_LIB_REFS = System System.Core
TEST_MCS_FLAGS =

LIBRARY_WARN_AS_ERROR = yes

SUBDIRS = FunctionalTests UnitTests

include ../../build/library.make

# TEST_RUNTIME := MONO_URI_DOTNETRELATIVEORABSOLUTE=true $(TEST_RUNTIME)

xunit-test-local: xunit-test-recursive

run-xunit-test-local: run-xunit-test-recursive

check: check-recursive

