thisdir = class/Mono.Cecil.Mdb
include ../../build/rules.make

LIBRARY = Mono.Cecil.Mdb.dll
LIBRARY_SNK = ../mono.snk
LIBRARY_PACKAGE = none

LIB_REFS = System Mono.Cecil
LIB_MCS_FLAGS = /r:$(corlib) /d:CECIL -keyfile:$(LIBRARY_SNK)

NO_TEST = yes
NO_INSTALL = yes

include ../../build/library.make
