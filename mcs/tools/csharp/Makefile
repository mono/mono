thisdir = tools/csharp
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:$(topdir)/class/lib/$(PROFILE)/gmcs.exe

ifeq (net_2_0, $(PROFILE))
PROGRAM = $(topdir)/class/lib/$(PROFILE)/csharp.exe
else
PROGRAM = 
endif

DISTFILES = repl.txt csharp.exe.sources

CLEAN_FILES = csharp.exe *.mdb

include ../../build/executable.make
