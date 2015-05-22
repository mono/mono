thisdir = class/System.Messaging
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Messaging.dll
LIB_REFS = System System.Xml Mono.Messaging

XAMMAC_4_5 := $(filter net_4_5_xammac, $(PROFILE))
ifndef XAMMAC_4_5
LIB_REFS += System.Drawing System.Windows.Forms System.Configuration.Install
endif

LIB_MCS_FLAGS = /resource:System.Messaging/MessageQueue.resx
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 \
		/r:nunit.mocks.dll

EXTRA_DISTFILES = System.Messaging/MessageQueue.resx

ifdef MESSAGING_NO_WINFORMS
LIB_MCS_FLAGS += -d:NO_WINFORMS_DEPENDENCY
endif

include ../../build/library.make
