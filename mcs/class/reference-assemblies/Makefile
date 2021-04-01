thisdir = class/reference-assemblies

include ../../build/rules.make

all-local:

PROFILE_DIR=$(DESTDIR)$(mono_libdir)/mono

build-reference-assemblies:
	$(MAKE) -C ../../../external/binary-reference-assemblies CSC="MONO_PATH=$(abspath $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)) $(INTERNAL_CSC)"

install-local:
	$(MKINSTALLDIRS) $(PROFILE_DIR)/2.0-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/3.5-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.0-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.5-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.5.1-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.5.2-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.6-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.6.1-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.6.2-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.7-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.7.1-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.7.2-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.8-api
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.5-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.5.1-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.5.2-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.6-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.6.1-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.6.2-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.7-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.7.1-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.7.2-api/Facades
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.8-api/Facades

	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v2.0/*.dll $(PROFILE_DIR)/2.0-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v3.5/*.dll $(PROFILE_DIR)/3.5-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.0/*.dll $(PROFILE_DIR)/4.0-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.5/*.dll $(PROFILE_DIR)/4.5-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.5.1/*.dll $(PROFILE_DIR)/4.5.1-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.5.2/*.dll $(PROFILE_DIR)/4.5.2-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.6/*.dll $(PROFILE_DIR)/4.6-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.6.1/*.dll $(PROFILE_DIR)/4.6.1-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.6.2/*.dll $(PROFILE_DIR)/4.6.2-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.7/*.dll $(PROFILE_DIR)/4.7-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.7.1/*.dll $(PROFILE_DIR)/4.7.1-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.7.2/*.dll $(PROFILE_DIR)/4.7.2-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.8/*.dll $(PROFILE_DIR)/4.8-api

	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.5/Facades/*.dll $(PROFILE_DIR)/4.5-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.5.1/Facades/*.dll $(PROFILE_DIR)/4.5.1-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.5.2/Facades/*.dll $(PROFILE_DIR)/4.5.2-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.6/Facades/*.dll $(PROFILE_DIR)/4.6-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.6.1/Facades/*.dll $(PROFILE_DIR)/4.6.1-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.6.2/Facades/*.dll $(PROFILE_DIR)/4.6.2-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.7/Facades/*.dll $(PROFILE_DIR)/4.7-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.7.1/Facades/*.dll $(PROFILE_DIR)/4.7.1-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.7.2/Facades/*.dll $(PROFILE_DIR)/4.7.2-api/Facades
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/v4.8/Facades/*.dll $(PROFILE_DIR)/4.8-api/Facades

	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/2.0-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.0-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.5-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.5.1-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.5.2-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.6-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.6.1-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.6.2-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.7-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.7.1-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.7.2-api
	$(INSTALL_LIB) ../../../external/binary-reference-assemblies/mono/*.dll $(PROFILE_DIR)/4.8-api
	rm -f $(PROFILE_DIR)/4.7.1-api/ICSharpCode.SharpZipLib.dll
	rm -f $(PROFILE_DIR)/4.7.2-api/ICSharpCode.SharpZipLib.dll
	rm -f $(PROFILE_DIR)/4.8-api/ICSharpCode.SharpZipLib.dll

	# Unfortunately, a few programs (most notably NUnit and FSharp) have hardcoded checks for <prefix>/lib/mono/4.0/mscorlib.dll or Mono.Posix.dll,
	# so we need to place something there or those tools break. We decided to symlink to the reference assembly for now.
	# See https://bugzilla.xamarin.com/show_bug.cgi?id=38331 and https://bugzilla.xamarin.com/show_bug.cgi?id=41052
	$(MKINSTALLDIRS) $(PROFILE_DIR)/4.0
	$(if $(filter $(BUILD_PLATFORM),win32),CYGWIN=winsymlinks:nativestrict) ln -sf ../4.0-api/mscorlib.dll $(PROFILE_DIR)/4.0/mscorlib.dll
	$(if $(filter $(BUILD_PLATFORM),win32),CYGWIN=winsymlinks:nativestrict) ln -sf ../4.0-api/Mono.Posix.dll $(PROFILE_DIR)/4.0/Mono.Posix.dll

DISTFILES =	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.8/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.7.2/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.7.1/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.7/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.6.2/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.6.1/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.6/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.5.2/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.5.1/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.5/Facades/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.8/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.7.2/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.7.1/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.7/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.6.2/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.6.1/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.6/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.5.2/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.5.1/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.5/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v4.0/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v3.5/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/v2.0/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/mono/*.dll)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.8/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.7.2/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.7.1/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.7/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.6.2/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.6.1/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.6/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.5.2/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.5.1/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.5/Facades/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.8/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.7.2/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.7.1/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.7/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.6.2/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.6.1/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.6/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.5.2/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.5.1/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.5/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v4.0/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v3.5/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/v2.0/*.cs)	\
	$(wildcard ../../../external/binary-reference-assemblies/src/mono/*.cs)	\
	../../../external/binary-reference-assemblies/v4.8/Makefile	\
	../../../external/binary-reference-assemblies/v4.7.2/Makefile	\
	../../../external/binary-reference-assemblies/v4.7.1/Makefile	\
	../../../external/binary-reference-assemblies/v4.7/Makefile	\
	../../../external/binary-reference-assemblies/v4.6.2/Makefile	\
	../../../external/binary-reference-assemblies/v4.6.1/Makefile	\
	../../../external/binary-reference-assemblies/v4.6/Makefile	\
	../../../external/binary-reference-assemblies/v4.5.2/Makefile	\
	../../../external/binary-reference-assemblies/v4.5.1/Makefile	\
	../../../external/binary-reference-assemblies/v4.5/Makefile	\
	../../../external/binary-reference-assemblies/v4.0/Makefile	\
	../../../external/binary-reference-assemblies/v3.5/Makefile	\
	../../../external/binary-reference-assemblies/v2.0/Makefile	\
	../../../external/binary-reference-assemblies/mono/Makefile	\
	../../../external/binary-reference-assemblies/Makefile	\
	Makefile

dist-local: dist-default
