thisdir = class/Facades/System.Runtime
SUBDIRS = 
include ../../../build/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Runtime.dll
LIB_MCS_FLAGS = /r:System /r:System.Core /r:System.ComponentModel.Composition

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include ../../../build/library.make
