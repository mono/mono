# -*- makefile -*-
#
# The rules for building our class libraries.
#
# The NO_TEST stuff is not too pleasant but whatcha
# gonna do.

# All the dep files now land in the same directory so we
# munge in the library name to keep the files from clashing.

core_sourcefile = $(LIBRARY).sources
PLATFORM_excludes := $(wildcard $(LIBRARY).$(PLATFORM)-excludes)

ifdef PLATFORM_excludes
sourcefile = $(depsdir)/$(LIBRARY).$(PLATFORM)-sources
$(sourcefile): $(core_sourcefile) $(PLATFORM_excludes)
	cat $(core_sourcefile) $(PLATFORM_excludes) | sort | uniq -u >$@
else
sourcefile = $(core_sourcefile)
endif

ifdef PLATFORM_CHANGE_SEPARATOR_CMD
response = $(depsdir)/$(PROFILE)_$(LIBRARY).response
else
response = $(sourcefile)
endif

ifndef LIBRARY_NAME
LIBRARY_NAME = $(LIBRARY)
endif

makefrag = $(depsdir)/$(PROFILE)_$(LIBRARY).makefrag
the_lib = $(topdir)/class/lib/$(PROFILE)/$(LIBRARY_NAME)
the_pdb = $(patsubst %.dll,%.pdb,$(the_lib))

ifndef NO_TEST
test_nunitfw = $(topdir)/class/lib/$(PROFILE)/nunit.framework.dll 
test_nunitcore = $(topdir)/class/lib/$(PROFILE)/nunit.core.dll 
test_nunitutil = $(topdir)/class/lib/$(PROFILE)/nunit.util.dll 
test_nunit_dep = $(test_nunitfw) $(test_nunitcore) $(test_nunitutil)
test_nunit_ref = -r:$(test_nunitfw) -r:$(test_nunitcore) -r:$(test_nunitutil)

ifndef test_against
test_against = $(the_lib)
test_dep = $(the_lib)
endif

ifndef test_lib
test_lib = $(patsubst %.dll,%_test.dll,$(LIBRARY))
endif
test_pdb = $(patsubst %.dll,%.pdb,$(test_lib))
test_sourcefile = $(test_lib).sources
test_response = $(depsdir)/$(PROFILE)_$(test_lib).response
test_makefrag = $(depsdir)/$(PROFILE)_$(test_lib).makefrag
test_flags = /r:$(test_against) $(test_nunit_ref) $(TEST_MCS_FLAGS)

ifndef btest_lib
btest_lib = $(patsubst %.dll,%_btest.dll,$(LIBRARY))
endif
btest_pdb = $(patsubst %.dll,%.pdb,$(btest_lib))
btest_sourcefile = $(btest_lib).sources
btest_response = $(depsdir)/$(btest_lib).response
btest_makefrag = $(depsdir)/$(btest_lib).makefrag
btest_flags = /r:$(test_against) $(test_nunit_ref) $(TEST_MBAS_FLAGS)

ifndef HAVE_CS_TESTS
HAVE_CS_TESTS := $(wildcard $(btest_sourcefile))
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
	$(RUNTIME) $(gacutil) /i $(the_lib) /f /root $(DESTDIR)$(prefix)/lib /package $(PACKAGE)

uninstall-local: $(gacutil)
	$(RUNTIME) $(gacutil) /u `echo $(LIBRARY_NAME) | sed 's,.dll$,,'`

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
	-rm -f $(the_lib) $(makefrag) $(test_lib) \
	       $(test_makefrag) $(test_response) \
	       $(the_pdb) $(test_pdb) $(CLEAN_FILES) \
	       TestResult.xml
ifdef PLATFORM_excludes
	-rm -rf $(sourcefile)
endif
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	-rm -rf $(response)
endif


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

DISTFILES = $(core_sourcefile) $(test_sourcefile) $(EXTRA_DISTFILES)

TEST_FILES = 

ifdef HAVE_CS_TESTS
TEST_FILES += `sed 's,^,Test/,' $(test_sourcefile)`
endif
ifdef HAVE_VB_TESTS
TEST_FILES += `sed 's,^,Test/,' $(btest_sourcefile)`
endif

dist-local: dist-default
	for f in `cat $(core_sourcefile)` $(TEST_FILES) ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

ifndef LIBRARY_COMPILE
LIBRARY_COMPILE = $(CSCOMPILE)
endif

# Fun with dependency tracking

$(the_lib): $(makefrag) $(response)
	$(LIBRARY_COMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) /target:library /out:$@ @$(response)

$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@sed 's,^,$(the_lib): ,' $< >$@

ifdef PLATFORM_CHANGE_SEPARATOR_CMD
$(response): $(sourcefile)
	@echo Creating $@ ...
	@cat $(sourcefile) | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

-include $(makefrag)

# for now, don't give any /lib flags or set MONO_PATH, since we
# give a full path to the assembly.

ifdef HAVE_CS_TESTS

$(test_lib): $(test_makefrag) $(test_dep) $(test_response) $(test_nunit_dep)
	$(CSCOMPILE) /target:library /out:$@ $(test_flags) @$(test_response)

$(test_response): $(test_sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@sed 's,^,Test/,' $< |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@sed 's,^,Test/,' $< >$@
endif

$(test_makefrag): $(test_response)
	@echo Creating $@ ...
	@sed 's,^,$(test_lib): ,' $< >$@

-include $(test_makefrag)

endif

ifdef HAVE_VB_TESTS

$(btest_lib): $(btest_makefrag) $(test_dep) $(btest_response) $(test_nunit_dep)
	$(BASCOMPILE) /target:library /out:$@ $(btest_flags) @$(btest_response)

$(btest_response): $(btest_sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@sed 's,^,Test/,' $< |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@sed 's,^,Test/,' $< >$@
endif

$(btest_makefrag): $(btest_response)
	@echo Creating $@ ...
	@sed 's,^,$(btest_lib): ,' $< >$@

-include $(btest_makefrag)

endif
