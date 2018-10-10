thisdir = class/Mono.Cecil
include ../../build/rules.make

LIBRARY = Mono.Cecil.dll
KEYFILE = ../mono.snk
LIBRARY_PACKAGE = none

LIB_REFS = System.Core System

API_BUILD := $(filter build, $(PROFILE))

ifdef API_BUILD
TARGET_NET_REFERENCE = $(BOOTSTRAP_BIN_PROFILE)
LIBRARY_USE_INTERMEDIATE_FILE = 1
endif

LIB_MCS_FLAGS = -d:NET_4_0

NO_TEST = yes

include ../../build/library.make
