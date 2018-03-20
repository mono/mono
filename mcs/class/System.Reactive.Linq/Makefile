thisdir = class/System.Reactive.Linq
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Reactive.Linq.dll
LIB_REFS = System System.Core System.Reactive.Interfaces System.Reactive.Core
KEYFILE = ../reactive.pub
LIB_MCS_FLAGS = -d:SIGNED -delaysign -resource:Strings_Linq.resources,System.Reactive.Strings_Linq.resources

ifeq (true, $(GENERATE_RESOURCES))
LIB_MCS_FLAGS += /define:GENERATING_RESOURCES
endif

RESX_RESOURCES = \
	Strings_Linq.resources

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

TEST_MCS_FLAGS =

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

