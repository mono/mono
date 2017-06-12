# -*- makefile -*-
#
# The rules for building with msbuild.
#
# The NO_TEST stuff is not too pleasant but whatcha
# gonna do.

ifndef ASSEMBLY_NAME
ASSEMBLY_NAME = $(ASSEMBLY)
endif

the_assemblydir_base = $(topdir)/class/lib/$(PROFILE_DIRECTORY)/$(if $(ASSEMBLY_SUBDIR),$(ASSEMBLY_SUBDIR)/)

the_assemblydir = $(the_assemblydir_base)$(intermediate)
the_assembly    = $(the_assemblydir)$(ASSEMBLY_NAME)

ifeq ($(BUILD_PLATFORM), win32)
GACDIR = `cygpath -w $(mono_libdir)`
GACROOT = `cygpath -w $(DESTDIR)$(mono_libdir)`
test_flags += -d:WINDOWS
else
GACDIR = $(mono_libdir)
GACROOT = $(DESTDIR)$(mono_libdir)
endif

ifndef NO_BUILD
all-local: $(extra_targets)
endif

csproj-local:

install-local: all-local
test-local: all-local
uninstall-local:

ifdef NO_INSTALL
install-local uninstall-local:
	@:

else

aot_lib = $(the_assembly)$(PLATFORM_AOT_SUFFIX)
aot_libname = $(ASSEMBLY_NAME)$(PLATFORM_AOT_SUFFIX)

ifdef ASSEMBLY_INSTALL_DIR
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(ASSEMBLY_INSTALL_DIR)
	$(INSTALL_LIB) $(the_assembly) $(DESTDIR)$(ASSEMBLY_INSTALL_DIR)/$(ASSEMBLY_NAME)
	test ! -f $(the_assembly).mdb || $(INSTALL_LIB) $(the_assembly).mdb $(DESTDIR)$(ASSEMBLY_INSTALL_DIR)/$(ASSEMBLY_NAME).mdb
	test ! -f $(the_assembly:.dll=.pdb) || $(INSTALL_LIB) $(the_assembly:.dll=.pdb) $(DESTDIR)$(ASSEMBLY_INSTALL_DIR)/$(ASSEMBLY_NAME:.dll=.pdb)

ifdef PLATFORM_AOT_SUFFIX
	test ! -f $(aot_lib) || $(INSTALL_LIB) $(aot_lib) $(DESTDIR)$(ASSEMBLY_INSTALL_DIR)
endif

uninstall-local:
	-rm -f $(DESTDIR)$(ASSEMBLY_INSTALL_DIR)/$(ASSEMBLY_NAME) $(DESTDIR)$(ASSEMBLY_INSTALL_DIR)/$(ASSEMBLY_NAME).mdb $(DESTDIR)$(ASSEMBLY_INSTALL_DIR)/$(ASSEMBLY_NAME:.dll=.pdb)

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

ifndef ASSEMBLY_PACKAGE
ifdef ASSEMBLY_COMPAT
ASSEMBLY_PACKAGE = compat-$(FRAMEWORK_VERSION)
else
ASSEMBLY_PACKAGE = $(FRAMEWORK_VERSION)
endif
endif

ifneq (none, $(ASSEMBLY_PACKAGE))
package_flag = /package $(ASSEMBLY_PACKAGE)
endif

install-local: $(gacutil)
	$(GACUTIL) /i $(the_assembly) /f $(gacdir_flag) /root $(GACROOT) $(package_flag)

uninstall-local: $(gacutil)
	-$(GACUTIL) /u $(ASSEMBLY_NAME:.dll=) $(gacdir_flag) /root $(GACROOT) $(package_flag)

endif # ASSEMBLY_INSTALL_DIR
endif # NO_INSTALL

clean-local:
	-rm -rf $(tests_CLEAN_FILES) $(assembly_CLEAN_FILES) $(CLEAN_FILES)

test-local run-test-local run-test-ondotnet-local:
	@:

DISTFILES = $(wildcard $(ASSEMBLY:$(ASSEMBLY_EXT)=.csproj)) $(EXTRA_DISTFILES)

ASSEMBLY_EXT  = $(suffix $(ASSEMBLY))
include $(topdir)/build/tests.make

ifdef HAVE_CS_TESTS
DISTFILES += $(test_sourcefile)
endif

# make dist will collect files in .sources files from all profiles
dist-local: dist-default
	subs=' ' ; \
	for f in `$(topdir)/tools/removecomments.sh $(filter-out $(wildcard *_test.dll.sources) $(wildcard *_xtest.dll.sources) $(wildcard *exclude.sources),$(wildcard *.sources))` $(TEST_FILES) ; do \
	  case $$f in \
	  ../*) : ;; \
	  *.g.cs) : ;; \
	  *) dest=`dirname "$$f"` ; \
	     case $$subs in *" $$dest "*) : ;; *) subs=" $$dest$$subs" ; $(MKINSTALLDIRS) $(distdir)/$$dest ;; esac ; \
	     cp -p "$$f" $(distdir)/$$dest || exit 1 ;; \
	  esac ; done ; \
	for d in . $$subs ; do \
	  case $$d in .) : ;; *) test ! -f $$d/ChangeLog || cp -p $$d/ChangeLog $(distdir)/$$d ;; esac ; done

# The assembly

ifndef NO_BUILD
ifdef PLATFORM_AOT_SUFFIX

$(the_assembly)$(PLATFORM_AOT_SUFFIX): $(the_assembly)
	$(Q_AOT) MONO_PATH='$(the_assemblydir_base)' > $(PROFILE)_$(ASSEMBLY_NAME)_aot.log 2>&1 $(RUNTIME) $(AOT_BUILD_FLAGS) --debug $(the_assembly)

all-local-aot: $(the_assembly)$(PLATFORM_AOT_SUFFIX)

assembly_CLEAN_FILES += $(the_assembly)$(PLATFORM_AOT_SUFFIX) $(PROFILE)_$(ASSEMBLY_NAME)_aot.log

endif
endif

# for now, don't give any /lib flags or set MONO_PATH, since we
# give a full path to the assembly.

## Include corcompare stuff
include $(topdir)/build/corcompare.make

ifndef NO_BUILD
all-local: $(test_makefrag) $(btest_makefrag)
endif

$(test_response) $(test_makefrag) $(btest_response) $(btest_makefrag): $(topdir)/build/msbuild.make $(depsdir)/.stamp

## Documentation stuff

doc-update-local: $(the_assemblydir)/.doc-stamp

$(the_assemblydir)/.doc-stamp: $(the_assembly)
	$(MDOC_UP) $(the_assembly)
	@echo "doc-stamp" > $@

gen-deps:

update-corefx-sr: $(RESX_RESOURCE_STRING)
	MONO_PATH="$(topdir)/class/lib/$(BUILD_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_PROFILE)/resx2sr.exe $(RESX_RESOURCE_STRING) >corefx/SR.cs
