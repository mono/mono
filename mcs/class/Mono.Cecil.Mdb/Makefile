thisdir = class/Mono.Cecil.Mdb
include ../../build/rules.make

LIBRARY = Mono.Cecil.Mdb.dll
KEYFILE = ../mono.snk
LIBRARY_PACKAGE = none

API_BUILD := $(filter basic build, $(PROFILE))

ifdef API_BUILD
DEFAULT_REFERENCES =
API_BIN_REFS = System mscorlib
else
LIB_REFS = System
endif

LIB_REFS += Mono.Cecil

LIB_MCS_FLAGS = -d:CECIL

NO_TEST = yes
NO_INSTALL = yes

include ../../build/library.make
