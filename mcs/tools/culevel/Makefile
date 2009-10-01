thisdir = tools/culevel
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = 
PROGRAM = $(topdir)/class/lib/$(PROFILE)/culevel.exe

CLEAN_FILES = culevel.exe culevel.exe.mdb

ifeq (net_2_0_bootstrap, $(PROFILE))
all-local: $(topdir)/class/lib/$(PROFILE)/culevel.exe
endif

include ../../build/executable.make
