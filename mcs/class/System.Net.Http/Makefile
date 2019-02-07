thisdir = class/System.Net.Http

include ../../build/rules.make

LIBRARY = System.Net.Http.dll

LIB_REFS = System.Core System
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = $(EXTRA_LIB_MCS_FLAGS) -unsafe -nowarn:436
ifeq (monodroid,$(PROFILE))
LIB_MCS_FLAGS += -d:XAMARIN_MODERN
endif

TEST_LIB_REFS = System System.Core
TEST_MCS_FLAGS =

LIBRARY_WARN_AS_ERROR = yes

EXTRA_DISTFILES = \
	TestData/testservereku.contoso.com.pfx

ifndef SOCKETSHTTPHANDLER
TEST_MCS_FLAGS += -d:LEGACY_HTTPCLIENT
LIB_MCS_FLAGS += -d:LEGACY_HTTPCLIENT
endif

SUBDIRS = FunctionalTests UnitTests

include ../../build/library.make

# TEST_RUNTIME := MONO_URI_DOTNETRELATIVEORABSOLUTE=true $(TEST_RUNTIME)

xunit-test-local: xunit-test-recursive

run-xunit-test-local: run-xunit-test-recursive

check: check-recursive

