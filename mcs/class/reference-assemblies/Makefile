thisdir = class/reference-assemblies

include ../../build/rules.make

all-local:

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.0/* $(DESTDIR)$(LIBRARY_INSTALL_DIR)

DISTFILES = $(wildcard ../../../external/binary-reference-assemblies/v4.0/*) Makefile

dist-local: dist-default
