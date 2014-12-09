thisdir = tools/symbolicate
SUBDIRS =
include ../../build/rules.make

PROGRAM = symbolicate.exe

LOCAL_MCS_FLAGS = \
	/r:Mono.Cecil.dll	\
	/r:Mono.CompilerServices.SymbolWriter.dll \
	/r:System.Xml

include ../../build/executable.make
