thisdir = tools/gacutil
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = /r:Mono.Security.dll

ifeq (net_1_1_bootstrap, $(PROFILE))
PROGRAM = $(topdir)/class/lib/$(PROFILE)/gacutil.exe
else
PROGRAM = gacutil.exe
endif

include ../../build/executable.make
