thisdir = class/Mono.CSharp.Debugger
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.CompilerServices.SymbolWriter.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

LIB_MCS_FLAGS = /r:$(corlib)
NO_TEST = yes

ifeq (win32default, $(PLATFORM)$(PROFILE))
LIBRARY_COMPILE = MONO_PATH="$(topdir)/class/lib/$(PROFILE);$$MONO_PATH" $(INTERNAL_MCS) $(USE_MCS_FLAGS)
endif

include ../../build/library.make
