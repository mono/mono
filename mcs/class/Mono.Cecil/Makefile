thisdir = class/Mono.Cecil
include ../../build/rules.make

LIBRARY = Mono.Cecil.dll
LIBRARY_SNK = ../mono.snk
LIBRARY_PACKAGE = none

LIB_REFS = System.Core
LIB_MCS_FLAGS = -keyfile:$(LIBRARY_SNK) -d:NET_4_0 /publicsign

NO_TEST = yes

include ../../build/library.make
