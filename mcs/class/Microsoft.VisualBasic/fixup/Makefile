thisdir = class/Microsoft.VisualBasic/fixup

DISTFILES = fixup.pl

include ../../../build/rules.make

with_mono_path = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"
PERL = perl

# ----------------------------------------------
# Minor hack.  Copied from build/library.make

ifeq ($(PLATFORM), win32)
GACDIR = `cygpath -w $(mono_libdir)`
GACROOT = `cygpath -w $(DESTDIR)$(mono_libdir)`
test_flags += -d:WINDOWS
else
GACDIR = $(mono_libdir)
GACROOT = $(DESTDIR)$(mono_libdir)
endif

ifdef NO_INSTALL
GACUTIL = :
else
gacutil = $(topdir)/class/lib/net_1_1_bootstrap/gacutil.exe
GACUTIL = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(gacutil)
endif

ifdef NO_SIGN_ASSEMBLY
SN = :
else
sn = $(topdir)/class/lib/net_1_1_bootstrap/sn.exe
SN = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(sn)
SNFLAGS = -q -R
endif

# ----------------------------------------------

ilasm = $(topdir)/class/lib/net_1_1_bootstrap/ilasm.exe
ILASM = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(ilasm)

the_lib = $(PROFILE)/Microsoft.VisualBasic.dll
the_il = $(the_lib:.dll=.il)

all-local: $(the_lib)

$(the_lib): $(the_il) Microsoft.VisualBasic.VBUtils.resources
	$(ILASM) $(IL_FLAGS) /out:$@ $<
	$(SN) $(SNFLAGS) $@ $(topdir)/class/mono.snk

$(the_il): $(topdir)/class/lib/$(PROFILE)/Microsoft.VisualBasic.dll fixup.pl
	$(with_mono_path) $(ILDISASM) $< > $(PROFILE)/t1 || { rm -f $(PROFILE)/t1; exit 1; }
	$(PERL) ./fixup.pl $(PROFILE)/t1 > $(PROFILE)/t2 && rm -f $(PROFILE)/t1
	mv $(PROFILE)/t2 $@

Microsoft.VisualBasic.VBUtils.resources: ../Microsoft.VisualBasic/VBUtils.resources
	cp $< $@

test-local run-test-local run-test-ondotnet-local:
	@:

# ----------------------------------------------
# Minor hack.  Copied from build/library.make

install-local: $(the_lib)

uninstall-local:

ifndef NO_INSTALL

ifdef LIBRARY_INSTALL_DIR
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(the_lib) $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME)
	test ! -f $(the_lib).mdb || $(INSTALL_LIB) $(the_lib).mdb $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME).mdb

uninstall-local:
	-rm -f $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME) $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME).mdb

else

# If RUNTIME_HAS_CONSISTENT_GACDIR is set, it implies that the internal GACDIR
# of the runtime is the same as the GACDIR we want.  So, we don't need to pass it
# to gacutil.  Note that the GACDIR we want may not be the same as the value of
# GACDIR set above, since the user could have overridden the value of $(prefix).
#
# This makes a difference only when we're building from the mono/ tree, since we
# have to ensure that the internal GACDIR of the in-tree runtime matches where we
# install the DLLs.

ifndef RUNTIME_HAS_CONSISTENT_GACDIR
gacdir_flag = /gacdir $(GACDIR)
endif

install-local: $(gacutil)
	$(GACUTIL) /i $(the_lib) /f $(gacdir_flag) /root $(GACROOT) /package $(FRAMEWORK_VERSION)

uninstall-local: $(gacutil)
	-$(GACUTIL) /u Microsoft.VisualBasic $(gacdir_flag) /root $(GACROOT) /package $(FRAMEWORK_VERSION)

endif
endif
# ----------------------------------------------

clean-local:
	rm -f $(the_lib)* $(the_il) Microsoft.VisualBasic.VBUtils.resources

dist-local: dist-default
	$(MKINSTALLDIRS) $(distdir)/default $(distdir)/net_2_0
