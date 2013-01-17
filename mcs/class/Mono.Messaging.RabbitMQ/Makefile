thisdir = class/Mono.Messaging.RabbitMQ
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Messaging.RabbitMQ.dll
LIB_MCS_FLAGS = /r:System.dll \
		/r:System.Messaging.dll \
		/r:Mono.Messaging.dll \
		/r:RabbitMQ.Client \
		-nowarn:618

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 \
	/r:nunit.mocks.dll

include ../../build/library.make

export MONO_MESSAGING_PROVIDER=Mono.Messaging.RabbitMQ.RabbitMQMessagingProvider,Mono.Messaging.RabbitMQ