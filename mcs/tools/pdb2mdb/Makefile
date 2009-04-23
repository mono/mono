thisdir = tools/pdb2mdb
SUBDIRS =
include ../../build/rules.make

PROGRAM = pdb2mdb.exe

LOCAL_MCS_FLAGS = \
	/r:$(topdir)/class/lib/net_1_1/Mono.Cecil.dll	\
	/r:Mono.CompilerServices.SymbolWriter.dll

EXTRA_DISTFILES = LICENSE

include ../../build/executable.make
