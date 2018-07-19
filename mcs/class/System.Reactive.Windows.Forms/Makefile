thisdir = class/System.Reactive.Windows.Forms
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Reactive.Windows.Forms.dll
LIB_REFS = System System.Core System.Reactive.Interfaces System.Reactive.Core System.Reactive.Linq System.Windows.Forms
KEYFILE = ../reactive.pub
LIB_MCS_FLAGS = -d:SIGNED -delaysign

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NO_TASK_DELAY -d:HAS_AWAIT
endif

NET_4_5 := $(filter 4.5, $(FRAMEWORK_VERSION))
ifdef NET_4_5
LIB_MCS_FLAGS += -d:HAS_EDI -d:PREFERASYNC -d:PREFER_ASYNC -d:HAS_AWAIT
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

INSTALL_PROFILE := $(filter net_4_x, $(PROFILE))
ifndef INSTALL_PROFILE
NO_INSTALL = yes
endif

NO_TEST = yes

include ../../build/library.make
