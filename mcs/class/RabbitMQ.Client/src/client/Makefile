thisdir = class/RabbitMQ.Client/src/client
SUBDIRS = 
include ../../../../build/rules.make

LIBRARY = RabbitMQ.Client.dll
LIB_REFS = System System.Xml
KEYFILE = ../../../msfinal.pub
LIB_MCS_FLAGS = -nowarn:618
NO_TEST = yes

BUILT_SOURCES = ../../docs/specs/$(PROFILE)-api-0-9.cs \
	../../docs/specs/$(PROFILE)-api-0-8.cs \
	../../docs/specs/$(PROFILE)-api-qpid-0-8.cs


include ../../../../build/library.make

