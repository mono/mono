thisdir = class/System.Reactive.PlatformServices
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Reactive.PlatformServices.dll
LIB_REFS = System System.Core System.Reactive.Interfaces System.Reactive.Core System.Reactive.Linq
KEYFILE = ../reactive.pub
LIB_MCS_FLAGS = -d:SIGNED -delaysign -resource:Strings_PlatformServices.resources,System.Reactive.Strings_PlatformServices.resources

ifeq (true, $(GENERATE_RESOURCES))
LIB_MCS_FLAGS += /define:GENERATING_RESOURCES
endif

RESX_RESOURCES = \
	Strings_PlatformServices.resources

CLEAN_FILES += $(RESX_RESOURCES)

RESOURCES = $(RESX_RESOURCES)

PREBUILT = $(RESX_RESOURCES:=.prebuilt)

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NO_TASK_DELAY -d:HAS_AWAIT
endif

NET_4_5 := $(filter 4.5, $(FRAMEWORK_VERSION))
ifdef NET_4_5
LIB_MCS_FLAGS += -d:HAS_EDI -d:PREFERASYNC -d:PREFER_ASYNC -d:HAS_AWAIT
endif

NO_TASK_DELAY := $(filter 4.5 2.1, $(FRAMEWORK_VERSION))
ifndef NO_TASK_DELAY
LIB_MCS_FLAGS += -d:NO_TASK_DELAY
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)
TEST_LIB_REFS = Mono.Reactive.Tests

EXTRA_DISTFILES = $(RESX_RESOURCES:.resources=.resx) $(PREBUILT)

INSTALL_PROFILE := $(filter net_4_x, $(PROFILE))
ifndef INSTALL_PROFILE
NO_INSTALL = yes
endif

NO_TEST = yes

include ../../build/library.make

$(the_lib): $(RESOURCES)

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@

$(PREBUILT): %.prebuilt: %
	cp $* $@

dist-default: $(PREBUILT)

