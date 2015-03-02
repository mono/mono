thisdir = class/Mono.Cecil
include ../../build/rules.make

LIBRARY = Mono.Cecil.dll
LIBRARY_SNK = ../mono.snk
LIBRARY_PACKAGE = none

LIB_REFS = System.Core
LIB_MCS_FLAGS = /r:$(corlib) -keyfile:$(LIBRARY_SNK) -d:NET_3_5

NO_TEST = yes

include ../../build/library.make
