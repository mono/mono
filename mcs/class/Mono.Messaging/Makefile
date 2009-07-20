thisdir = class/Mono.Messaging
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Messaging.dll

ifeq (1, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS = /r:System.dll
else
LIB_MCS_FLAGS = /r:System.dll /r:System.Configuration
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 /r:System.Messaging.dll /r:nunit.mocks.dll

include ../../build/library.make
