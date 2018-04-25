thisdir = class/Mono.Cecil.Mdb
include ../../build/rules.make

LIBRARY = Mono.Cecil.Mdb.dll
LIBRARY_SNK = ../mono.snk
LIBRARY_PACKAGE = none

LIB_REFS = System Mono.Cecil
LIB_MCS_FLAGS = /d:CECIL -keyfile:$(LIBRARY_SNK) -publicsign

NO_TEST = yes
NO_INSTALL = yes

include ../../build/library.make
