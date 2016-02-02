thisdir = class/reference-assemblies

include ../../build/rules.make

all-local:

PROFILE_DIR=$(DESTDIR)$(mono_libdir)/mono

install-local:
	$(MKINSTALLDIRS) $(PROFILE_DIR)/2.0-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/3.5-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.0-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.5-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.5-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v2.0/*.dll $(PROFILE_DIR)/2.0-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v3.5/*.dll $(PROFILE_DIR)/3.5-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.0/*.dll $(PROFILE_DIR)/4.0-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.5/*.dll $(PROFILE_DIR)/4.5-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.5/Facades/*.dll $(PROFILE_DIR)/4.5-api/Facades

DISTFILES = $(wildcard ../../../external/binary-reference-assemblies/v4.5/Facades/*.dll) $(wildcard ../../../external/binary-reference-assemblies/v4.5/*.dll) $(wildcard ../../../external/binary-reference-assemblies/v4.0/*.dll) $(wildcard ../../../external/binary-reference-assemblies/v3.5/*.dll) $(wildcard ../../../external/binary-reference-assemblies/v2.0/*.dll) Makefile

dist-local: dist-default
