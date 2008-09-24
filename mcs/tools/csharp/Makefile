thisdir = tools/csharp
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:$(topdir)/class/lib/$(PROFILE)/gmcs.exe -r:$(topdir)/class/lib/$(PROFILE)/Mono.Posix.dll -r:Mono.Management.dll

PROGRAM = $(topdir)/class/lib/$(PROFILE)/csharp.exe

DISTFILES = repl.txt

CLEAN_FILES = csharp.exe *.mdb

include ../../build/executable.make
