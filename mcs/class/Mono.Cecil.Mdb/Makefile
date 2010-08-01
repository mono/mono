thisdir = class/Mono.Cecil.Mdb
include ../../build/rules.make

LIBRARY = Mono.Cecil.Mdb.dll
LIBRARY_SNK = ../mono.snk
LIBRARY_PACKAGE = none

CECIL = $(topdir)/class/lib/$(PROFILE)/Mono.Cecil.dll

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:$(CECIL) /d:CECIL -keyfile:$(LIBRARY_SNK)

NO_TEST = yes

ifneq (net_2_0, $(PROFILE))
NO_INSTALL = yes
endif

include ../../build/library.make
