thisdir = class/Mono.Messaging.RabbitMQ
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Messaging.RabbitMQ.dll
LIB_REFS = System System.Messaging Mono.Messaging RabbitMQ.Client
KEYFILE = ../mono.pub
LIB_MCS_FLAGS = -nowarn:618

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169
TEST_LIB_REFS =

include ../../build/library.make

export MONO_MESSAGING_PROVIDER=Mono.Messaging.RabbitMQ.RabbitMQMessagingProvider,Mono.Messaging.RabbitMQ
