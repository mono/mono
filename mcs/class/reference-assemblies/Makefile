thisdir = class/reference-assemblies

include ../../build/rules.make

all-local:

PROFILE_DIR=$(DESTDIR)$(mono_libdir)/mono/

install-local:
	$(MKINSTALLDIRS) $(PROFILE_DIR)/2.0
	$(MKINSTALLDIRS) $(PROFILE_DIR)/3.5
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.0
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v2.0/* $(PROFILE_DIR)/2.0
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v3.5/* $(PROFILE_DIR)/3.5
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.0/* $(PROFILE_DIR)/4.0

DISTFILES = $(wildcard ../../../external/binary-reference-assemblies/v4.0/*) $(wildcard ../../../external/binary-reference-assemblies/v3.5/*) $(wildcard ../../../external/binary-reference-assemblies/v2.0/*) Makefile

dist-local: dist-default
