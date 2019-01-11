thisdir = tools/cil-stringreplacer
SUBDIRS =
include ../../build/rules.make

PROGRAM = cil-stringreplacer.exe
NO_INSTALL = yes

TARGET_NET_REFERENCE = $(BOOTSTRAP_BIN_PROFILE)
PROGRAM_USE_INTERMEDIATE_FILE = 1

LIB_REFS = System Mono.Cecil

ifdef MCS_MODE
LIB_REFS += Mono.Cecil.Mdb
endif

include ../../build/executable.make
