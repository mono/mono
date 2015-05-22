thisdir = class/System.Messaging
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Messaging.dll
LIB_MCS_FLAGS = /resource:System.Messaging/MessageQueue.resx \
		/r:System.dll \
		/r:System.Configuration.Install.dll \
		/r:System.Xml \
		/r:Mono.Messaging.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 \
		/r:nunit.mocks.dll

EXTRA_DISTFILES = System.Messaging/MessageQueue.resx

ifndef NO_SYSTEM_DRAWING_DEPENDENCY
LIB_MCS_FLAGS += /r:System.Drawing.dll
endif

ifdef NO_WINFORMS_DEPENDENCY
LIB_MCS_FLAGS += -d:NO_WINFORMS_DEPENDENCY
else
LIB_MCS_FLAGS += /r:System.Windows.Forms.dll
endif

include ../../build/library.make
