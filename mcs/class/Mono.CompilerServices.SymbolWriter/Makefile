thisdir = class/Mono.CompilerServices.SymbolWriter
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.CompilerServices.SymbolWriter.dll
LIB_MCS_FLAGS = -r:System.dll -lib:$(bare_libdir)

NO_TEST = yes

ifneq (basic, $(PROFILE))
USE_BOOT_COMPILE = yes
endif

ifdef USE_BOOT_COMPILE
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif

include ../../build/library.make

$(build_lib): $(bare_libdir)/System.dll

$(bare_libdir)/System.dll:
	(cd ../System; make $@)

.NOTPARALLEL: $(bare_libdir)/System.dll