thisdir = ilasm
SUBDIRS =
include ../build/rules.make

PROGRAM = ilasm.exe
BUILT_SOURCES = ILParser.cs
LOCAL_MCS_FLAGS = /lib:../class/lib/default /r:PEAPI.dll /r:Mono.CompilerServices.SymbolWriter.dll

CLEAN_FILES = parser/y.output

EXTRA_DISTFILES = \
	parser/ILParser.jay	\
	parser/ChangeLog	\
	scanner/ChangeLog	\
	tests/ChangeLog		\
	$(wildcard tests/*.il)

ILParser.cs: parser/ILParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs $< >$@

include ../build/executable.make
