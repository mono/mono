thisdir = ilasm
SUBDIRS =
include ../build/rules.make

PROGRAM = ilasm.exe
BUILT_SOURCES = parser/ILParser.cs
LOCAL_MCS_FLAGS = /lib:../class/lib/ /r:PEAPI.dll

CLEAN_FILES = parser/y.output

EXTRA_DISTFILES = \
	parser/ILParser.jay	\
	parser/ChangeLog	\
	scanner/ChangeLog	\
	tests/ChangeLog		\
	$(wildcard tests/*.il)

parser/ILParser.cs: parser/ILParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs $< >$@

include ../build/executable.make
