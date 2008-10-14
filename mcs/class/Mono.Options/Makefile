thisdir = class/Mono.Options
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Options.dll
LIBRARY_PACKAGE = none

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll
TEST_MCS_FLAGS = /r:Mono.Posix.dll /r:System.dll

mono_options_DIR  = $(DESTDIR)$(mono_libdir)/mono-options
mono_options_DATA = Mono.Options/Options.cs

include ../../build/library.make

do-install:
	-$(MKINSTALLDIRS) $(mono_options_DIR)
	$(INSTALL) -m 644 $(mono_options_DATA) $(mono_options_DIR)

do-uninstall:
	-rm -Rf $(mono_options_DIR)
