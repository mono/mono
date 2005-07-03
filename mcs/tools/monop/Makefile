thisdir = tools/monop
SUBDIRS = 
include ../../build/rules.make

ifeq (net_2_0, $(PROFILE))
PROGRAM = monop2.exe
else
PROGRAM = monop.exe
endif

include ../../build/executable.make
