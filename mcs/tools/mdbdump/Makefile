thisdir = tools/mdbdump
SUBDIRS =
include ../../build/rules.make

PROGRAM = mdbdump.exe

LOCAL_MCS_FLAGS = \
	/r:System.Xml.dll \
	/r:Mono.Cecil.dll	\
	/r:Mono.CompilerServices.SymbolWriter.dll

include ../../build/executable.make
