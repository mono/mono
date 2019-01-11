thisdir = class/PEAPI
SUBDIRS = 
include ../../build/rules.make

LIBRARY = PEAPI.dll
LIB_MCS_FLAGS = -nowarn:414,618

LIB_REFS = System

API_BUILD := $(filter build, $(PROFILE))

ifdef API_BUILD
TARGET_NET_REFERENCE = $(BOOTSTRAP_BIN_PROFILE)
LIBRARY_USE_INTERMEDIATE_FILE = 1
else
LIB_MCS_FLAGS += -d:HAS_MONO_SECURITY
LIB_REFS += Mono.Security
endif

KEYFILE = ../mono.pub
NO_TEST = yes

EXTRA_DISTFILES = README.txt

include ../../build/library.make
