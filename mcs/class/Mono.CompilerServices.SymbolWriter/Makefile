thisdir = class/Mono.CompilerServices.SymbolWriter
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.CompilerServices.SymbolWriter.dll
LIB_REFS = System
LIB_MCS_FLAGS =

NO_TEST = yes

include ../../build/library.make

$(the_lib): $(bare_libdir)/System.dll

$(bare_libdir)/System.dll:
	(cd ../System; make $@)

.NOTPARALLEL: $(bare_libdir)/System.dll
