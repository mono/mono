# -*- makefile -*-
#
# The rules for building our class libraries.
#
# The NO_TEST stuff is not too pleasant but whatcha
# gonna do.

# All the dep files now land in the same directory so we
# munge in the library name to keep the files from clashing.

sourcefile = $(LIBRARY).sources
PLATFORM_excludes := $(wildcard $(LIBRARY).$(PLATFORM)-excludes)

ifndef PLATFORM_excludes
ifeq (cat,$(PLATFORM_CHANGE_SEPARATOR_CMD))
response = $(sourcefile)
endif
endif

ifndef response
response = $(depsdir)/$(PROFILE)_$(LIBRARY).response
library_CLEAN_FILES += $(response)
endif

ifndef LIBRARY_NAME
LIBRARY_NAME = $(LIBRARY)
endif

makefrag = $(depsdir)/$(PROFILE)_$(LIBRARY).makefrag
the_lib = $(topdir)/class/lib/$(PROFILE)/$(LIBRARY_NAME)
the_pdb = $(the_lib:.dll=.pdb)
library_CLEAN_FILES += $(makefrag) $(the_lib) $(the_pdb)

ifndef NO_TEST
test_nunit_lib = nunit.framework.dll nunit.core.dll nunit.util.dll
test_nunit_dep = $(test_nunit_lib:%=$(topdir)/class/lib/$(PROFILE)/%)
test_nunit_ref = $(test_nunit_dep:%=-r:%)
library_CLEAN_FILES += TestResult.xml

ifndef test_against
test_against = $(the_lib)
test_dep = $(the_lib)
endif

ifndef test_lib
test_lib = $(LIBRARY:.dll=_test.dll)
endif
test_pdb = $(test_lib:.dll=.pdb)
test_sourcefile = $(test_lib).sources
test_response = $(depsdir)/$(PROFILE)_$(test_lib).response
test_makefrag = $(depsdir)/$(PROFILE)_$(test_lib).makefrag
test_flags = /r:$(test_against) $(test_nunit_ref) $(TEST_MCS_FLAGS)
library_CLEAN_FILES += $(test_lib) $(test_pdb) $(test_response) $(test_makefrag)

ifndef btest_lib
btest_lib = $(LIBRARY:.dll=_btest.dll)
endif
btest_pdb = $(btest_lib:.dll=.pdb)
btest_sourcefile = $(btest_lib).sources
btest_response = $(depsdir)/$(btest_lib).response
btest_makefrag = $(depsdir)/$(btest_lib).makefrag
btest_flags = /r:$(test_against) $(test_nunit_ref) $(TEST_MBAS_FLAGS)
library_CLEAN_FILES += $(btest_lib) $(btest_pdb) $(btest_response) $(btest_makefrag)

ifndef HAVE_CS_TESTS
HAVE_CS_TESTS := $(wildcard $(test_sourcefile))
endif
ifndef HAVE_VB_TESTS
HAVE_VB_TESTS := $(wildcard $(btest_sourcefile))
endif

endif

gacutil = $(topdir)/tools/gacutil/gacutil.exe
sn = $(topdir)/tools/security/sn.exe

PACKAGE = 1.0

ifeq ($(PROFILE), net_2_0)
PACKAGE = 2.0
endif

ifeq ($(PLATFORM), win32)
GACDIR = `cygpath -w $(DESTDIR)$(prefix)/lib`
else
GACDIR = $(DESTDIR)$(prefix)/lib
endif

all-local: $(the_lib)

install-local: $(the_lib) maybe-sign-lib

ifdef LIBRARY_INSTALL_DIR
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(the_lib) $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME)

uninstall-local:
	-rm -f $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME)

else

install-local: $(gacutil)
	$(RUNTIME) $(gacutil) /i $(the_lib) /f /root $(GACDIR) /package $(PACKAGE)

uninstall-local: $(gacutil)
	$(RUNTIME) $(gacutil) /u $(LIBRARY_NAME:.dll=)

$(gacutil):
	cd $(topdir)/tools/gacutil && $(MAKE)

endif

maybe-sign-lib:
ifndef NO_SIGN_ASSEMBLY
maybe-sign-lib: $(sn)
	$(RUNTIME) $(sn) -q -R $(the_lib) $(topdir)/class/mono.snk
endif

$(sn):
	cd $(topdir)/tools/security && $(MAKE) sn.exe

clean-local:
	-rm -f $(library_CLEAN_FILES) $(CLEAN_FILES)

test-local: $(the_lib)
	@:
run-test-local:
	@:
run-test-ondotnet-local:
	@:

ifndef NO_TEST
$(test_nunit_dep): $(topdir)/build/deps/nunit.stamp
	@if test -f $@; then :; else rm -f $<; $(MAKE) $<; fi
$(topdir)/build/deps/nunit.stamp:
	cd ${topdir}/nunit20 && $(MAKE)
	echo "stamp" >$@
endif

ifdef HAVE_CS_TESTS

test-local: $(test_lib)

run-test-local: run-test-lib

run-test-lib: test-local
	$(TEST_RUNTIME) $(TEST_HARNESS) $(test_lib)

run-test-ondotnet-local: run-test-ondotnet-lib

run-test-ondotnet-lib: test-local
	$(TEST_HARNESS) $(test_lib)
endif

ifdef HAVE_VB_TESTS

test-local: $(btest_lib)

run-test-local: run-btest-lib

run-btest-lib: test-local
	$(TEST_RUNTIME) $(TEST_HARNESS) $(btest_lib)

run-test-ondotnet-local: run-btest-ondotnet-lib

run-btest-ondotnet-lib: test-local
	$(TEST_HARNESS) $(btest_lib)

endif

DISTFILES = $(sourcefile) $(test_sourcefile) $(EXTRA_DISTFILES)

TEST_FILES = 

ifdef HAVE_CS_TESTS
TEST_FILES += `sed 's,^,Test/,' $(test_sourcefile)`
endif
ifdef HAVE_VB_TESTS
TEST_FILES += `sed 's,^,Test/,' $(btest_sourcefile)`
endif

dist-local: dist-default
	for f in `cat $(sourcefile)` $(TEST_FILES) ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

ifndef LIBRARY_COMPILE
LIBRARY_COMPILE = $(CSCOMPILE)
endif

ifndef TEST_COMPILE
TEST_COMPILE = $(CSCOMPILE)
endif

ifndef BTEST_COMPILE
BTEST_COMPILE = $(BASCOMPILE)
endif

# Fun with dependency tracking

$(the_lib): $(makefrag) $(response)
	$(LIBRARY_COMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) /target:library /out:$@ @$(response)

$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@sed 's,^,$(the_lib): ,' $< >$@

ifneq ($(response),$(sourcefile))
$(response): $(sourcefile) $(PLATFORM_excludes)
	@echo Creating $@ ...
	@sort $(sourcefile) $(PLATFORM_excludes) | uniq -u | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

-include $(makefrag)

# for now, don't give any /lib flags or set MONO_PATH, since we
# give a full path to the assembly.

ifdef HAVE_CS_TESTS

$(test_lib): $(test_makefrag) $(test_dep) $(test_response) $(test_nunit_dep)
	$(TEST_COMPILE) /target:library /out:$@ $(test_flags) @$(test_response)

$(test_response): $(test_sourcefile)
	@echo Creating $@ ...
	@sed 's,^,Test/,' $(test_sourcefile) | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

$(test_makefrag): $(test_response)
	@echo Creating $@ ...
	@sed 's,^,$(test_lib): ,' $< >$@

-include $(test_makefrag)

endif

ifdef HAVE_VB_TESTS

$(btest_lib): $(btest_makefrag) $(test_dep) $(btest_response) $(test_nunit_dep)
	$(BTEST_COMPILE) /target:library /out:$@ $(btest_flags) @$(btest_response)

$(btest_response): $(btest_sourcefile)
	@echo Creating $@ ...
	@sed 's,^,Test/,' $(btest_sourcefile) | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

$(btest_makefrag): $(btest_response)
	@echo Creating $@ ...
	@sed 's,^,$(btest_lib): ,' $< >$@

-include $(btest_makefrag)

endif

$(makefrag) $(test_makefrag) $(btest_makefrag): $(topdir)/build/library.make
