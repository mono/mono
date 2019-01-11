thisdir = class/Mono.Cecil.Mdb
include ../../build/rules.make

LIBRARY = Mono.Cecil.Mdb.dll
KEYFILE = ../mono.snk
LIBRARY_PACKAGE = none

API_BUILD := $(filter build, $(PROFILE))

ifdef API_BUILD
TARGET_NET_REFERENCE = $(BOOTSTRAP_BIN_PROFILE)
LIBRARY_USE_INTERMEDIATE_FILE = 1
endif

LIB_REFS = System Mono.Cecil

LIB_MCS_FLAGS = -d:CECIL

NO_TEST = yes
NO_INSTALL = yes

include ../../build/library.make
