thisdir = tools/csharp
SUBDIRS = 
include ../../build/rules.make

// 3021: CLS attribute not needed since assembly is not CLS compliant
NOWARNS = -nowarn:3021
LOCAL_MCS_FLAGS = -r:$(topdir)/class/lib/$(PROFILE)/gmcs.exe -r:$(topdir)/class/lib/$(PROFILE)/Mono.Posix.dll -r:Mono.Management.dll -unsafe $(NOWARNS)

PROGRAM = $(topdir)/class/lib/$(PROFILE)/csharp.exe

DISTFILES = repl.txt

CLEAN_FILES = csharp.exe *.mdb

include ../../build/executable.make
