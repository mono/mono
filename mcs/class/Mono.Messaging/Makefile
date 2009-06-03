thisdir = class/Mono.Messaging
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Messaging.dll
LIB_MCS_FLAGS = /r:System.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 /r:System.Messaging.dll /r:nunit.mocks.dll

include ../../build/library.make
