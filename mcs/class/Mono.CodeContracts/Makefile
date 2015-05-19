thisdir = class/Mono.CodeContracts
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.CodeContracts.dll

LIB_REFS = System System.Core Mono.Cecil Mono.Cecil.Mdb
LIB_MCS_FLAGS =
#-r:Mono.Cecil.Pdb.dll

TEST_MCS_FLAGS = -r:System.Core.dll -debug

include ../../build/library.make
