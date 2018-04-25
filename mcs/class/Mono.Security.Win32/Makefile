thisdir = class/Mono.Security.Win32
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Security.Win32.dll
KEYFILE = ../mono.pub
LIB_MCS_FLAGS = /unsafe
EXTRA_DISTFILES = README

ifneq (win32, $(PROFILE))
NO_TEST = yes
endif

include ../../build/library.make
