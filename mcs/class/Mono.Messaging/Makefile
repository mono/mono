thisdir = class/Mono.Messaging
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Messaging.dll

LIB_REFS = System System.Configuration
KEYFILE = ../mono.pub
LIB_MCS_FLAGS =

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169
TEST_LIB_REFS = System.Messaging

include ../../build/library.make
