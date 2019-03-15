thisdir = class/System.Messaging
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Messaging.dll
LIB_REFS = System System.Xml Mono.Messaging System.Configuration.Install

ifndef NO_SYSTEM_DRAWING_DEPENDENCY
LIB_REFS += System.Drawing
endif

KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = /resource:System.Messaging/MessageQueue.resx
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169
TEST_LIB_REFS =

EXTRA_DISTFILES = System.Messaging/MessageQueue.resx

ifdef NO_WINFORMS_DEPENDENCY
LIB_MCS_FLAGS += -d:NO_WINFORMS_DEPENDENCY
else
LIB_REFS += System.Windows.Forms
endif

include ../../build/library.make
