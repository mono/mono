thisdir = tools/cil-stringreplacer
SUBDIRS =
include ../../build/rules.make

PROGRAM = cil-stringreplacer.exe
NO_INSTALL = yes

ifeq ($(PROFILE),basic)
# It can be run using system .net during boostrap
TARGET_NET_REFERENCE = v4.6
# Trick to make it work during boostrap where it has to run with system
# assemblies not the ones in lib/basic folder
PROGRAM_USE_INTERMEDIATE_FILE = 1
endif

LIB_REFS = System Mono.Cecil

ifdef MCS_MODE
LIB_REFS += Mono.Cecil.Mdb
endif

include ../../build/executable.make
