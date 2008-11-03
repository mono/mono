thisdir = class/Mono.Options
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Options.dll
LIBRARY_PACKAGE = none

NO_INSTALL = yes

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll
TEST_MCS_FLAGS = /r:Mono.Posix.dll /r:System.dll

mono_sourcelibs_DIR  = $(DESTDIR)$(mono_libdir)/mono-source-libs
mono_options_DATA = Mono.Options/Options.cs

include ../../build/library.make

install-local: install-source

uninstall-local: uninstall-source

install-source:
	-$(MKINSTALLDIRS) $(mono_sourcelibs_DIR)
	$(INSTALL) -m 644 $(mono_options_DATA) $(mono_sourcelibs_DIR)

uninstall-source:
	-rm -f $(mono_options_DIR)/Options.cs

fixup-docs:
	for f in `find Documentation/en -name \*.xml` ; do \
		sed -i 's/NDesk.Options/Mono.Options/g' $$f ; \
	done
