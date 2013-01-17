thisdir = class/Mono.CodeContracts
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.CodeContracts.dll

LIB_MCS_FLAGS = -r:System.dll -r:System.Core.dll -r:Mono.Cecil.dll -r:Mono.Cecil.Mdb.dll 
#-r:Mono.Cecil.Pdb.dll

TEST_MCS_FLAGS = -r:System.Core.dll

include ../../build/library.make
