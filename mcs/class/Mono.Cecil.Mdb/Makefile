thisdir = class/Mono.Cecil.Mdb
include ../../build/rules.make

LIBRARY = Mono.Cecil.Mdb.dll
LIBRARY_PACKAGE = none

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:Mono.Cecil.dll /d:CECIL 

NO_TEST = yes

ifneq (net_2_0, $(PROFILE))
NO_INSTALL = yes
endif

include ../../build/library.make
