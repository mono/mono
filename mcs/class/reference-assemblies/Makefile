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

	# Unfortunately, a few programs (most notably NUnit and FSharp) have hardcoded checks for <prefix>/lib/mono/4.0/mscorlib.dll or Mono.Posix.dll,
	# so we need to place something there or those tools break. We decided to symlink to the reference assembly for now.
	# See https://bugzilla.xamarin.com/show_bug.cgi?id=38331 and https://bugzilla.xamarin.com/show_bug.cgi?id=41052
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.0
	ln -sf ../4.0-api/mscorlib.dll $(PROFILE_DIR)/4.0/mscorlib.dll
	ln -sf ../4.0-api/Mono.Posix.dll $(PROFILE_DIR)/4.0/Mono.Posix.dll

DISTFILES = $(wildcard ../../../external/binary-reference-assemblies/v4.5/Facades/*.dll) $(wildcard ../../../external/binary-reference-assemblies/v4.5/*.dll) $(wildcard ../../../external/binary-reference-assemblies/v4.0/*.dll) $(wildcard ../../../external/binary-reference-assemblies/v3.5/*.dll) $(wildcard ../../../external/binary-reference-assemblies/v2.0/*.dll) Makefile

dist-local: dist-default
