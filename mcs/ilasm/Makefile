thisdir = ilasm
SUBDIRS =
include ../build/rules.make

PROGRAM = ilasm.exe
BUILT_SOURCES = ILParser.cs
LIB_REFS = PEAPI Mono.CompilerServices.SymbolWriter Mono.Security

CLEAN_FILES = parser/y.output

EXTRA_DISTFILES = \
	parser/ILParser.jay	\
	$(wildcard tests/*.il)

ILParser.cs: parser/ILParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs $(CURDIR)/$< >$@

include ../build/executable.make

