thisdir = class/Mono.Cecil
include ../../build/rules.make

LIBRARY = Mono.Cecil.dll
KEYFILE = ../mono.snk
LIBRARY_PACKAGE = none

API_BUILD := $(filter basic build, $(PROFILE))

ifdef API_BUILD
DEFAULT_REFERENCES =
API_BIN_REFS = System.Core System mscorlib
LIBRARY_USE_INTERMEDIATE_FILE = 1
else
LIB_REFS = System.Core System
endif

LIB_MCS_FLAGS = -d:NET_4_0

NO_TEST = yes

include ../../build/library.make
