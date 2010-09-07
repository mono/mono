thisdir = class/Mono.CompilerServices.SymbolWriter
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.CompilerServices.SymbolWriter.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll
NO_TEST = yes

ifneq (basic, $(PROFILE))
USE_BOOT_COMPILE = yes
endif

ifdef USE_BOOT_COMPILE
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif

include ../../build/library.make
