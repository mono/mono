thisdir = class/System.Messaging
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Messaging.dll
LIB_MCS_FLAGS = /resource:System.Messaging/MessageQueue.resx \
		/r:System.dll \
		/r:System.Configuration.Install.dll \
		/r:System.Drawing.dll \
		/r:System.Windows.Forms.dll \
		/r:System.Xml \
		/r:Mono.Messaging.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 \
		/r:nunit.mocks.dll

EXTRA_DISTFILES = System.Messaging/MessageQueue.resx

ifdef MESSAGING_NO_WINFORMS
LIB_MCS_FLAGS += -d:NO_WINFORMS_DEPENDENCY
endif

include ../../build/library.make
