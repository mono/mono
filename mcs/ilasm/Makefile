thisdir = ilasm
SUBDIRS =
include ../build/rules.make

PROGRAM = $(topdir)/class/lib/$(PROFILE)/ilasm.exe
BUILT_SOURCES = ILParser.cs
LOCAL_MCS_FLAGS = /lib:../class/lib/$(PROFILE) /r:../class/lib/$(PROFILE)/PEAPI.dll /r:Mono.CompilerServices.SymbolWriter.dll /r:Mono.Security.dll

CLEAN_FILES = parser/y.output

EXTRA_DISTFILES = \
	parser/ILParser.jay	\
	parser/ChangeLog	\
	scanner/ChangeLog	\
	tests/ChangeLog		\
	$(wildcard tests/*.il)

ILParser.cs: parser/ILParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs $(CURDIR)/$< >$@

include ../build/executable.make

ifeq (net_1_1, $(PROFILE))
# Utility rule to make debugging somewhat easier.
all-local: ilasm.exe
ilasm.exe: $(PROGRAM)
	cp -p $< $@
	test ! -f $<.mdb || cp -p $<.mdb $@.mdb
endif
