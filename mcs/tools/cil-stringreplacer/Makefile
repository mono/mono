thisdir = tools/cil-stringreplacer
SUBDIRS =
include ../../build/rules.make

PROGRAM = cil-stringreplacer.exe
NO_INSTALL = yes

LIB_REFS = System Mono.Cecil

ifdef MCS_MODE
LIB_REFS += Mono.Cecil.Mdb
endif

include ../../build/executable.make
