thisdir = class/legacy/Mono.Cecil
include ../../../build/rules.make

LIBRARY = Mono.Cecil.dll
LIBRARY_SNK = ../../mono.snk
LIBRARY_PACKAGE = none
LIBRARY_SUBDIR = legacy

LIB_REFS = System.Core
LIB_MCS_FLAGS = -keyfile:$(LIBRARY_SNK) -d:NET_3_5 /publicsign

NO_TEST = yes

include ../../../build/library.make
