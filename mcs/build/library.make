# -*- makefile -*-
#
# The rules for building our class libraries.
#
# The NO_TEST stuff is not too pleasant but whatcha
# gonna do.

# All the dep files now land in the same directory so we
# munge in the library name to keep the files from clashing.

sourcefile = $(LIBRARY).sources
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

test_lib = $(patsubst %.dll,%_test.dll,$(LIBRARY))
test_pdb = $(patsubst %.dll,%.pdb,$(test_lib))
test_sourcefile = $(test_lib).sources
test_response = $(depsdir)/$(PROFILE)_$(test_lib).response
test_makefrag = $(depsdir)/$(PROFILE)_$(test_lib).makefrag
test_flags = /r:$(the_lib) $(test_nunit_ref) $(TEST_MCS_FLAGS)

btest_lib = $(patsubst %.dll,%_btest.dll,$(LIBRARY))
btest_pdb = $(patsubst %.dll,%.pdb,$(btest_lib))
btest_sourcefile = $(btest_lib).sources
btest_response = $(depsdir)/$(btest_lib).response
btest_makefrag = $(depsdir)/$(btest_lib).makefrag
btest_flags = /r:$(the_lib) $(test_nunit_ref) $(TEST_MBAS_FLAGS)

HAVE_CS_TESTS := $(wildcard $(test_sourcefile))
HAVE_VB_TESTS := $(wildcard $(btest_sourcefile))

endif

gacutil = $(topdir)/tools/gacutil/gacutil.exe
sn = $(topdir)/tools/security/sn.exe

PACKAGE = 1.0

ifeq ($(PROFILE), net_2_0)
PACKAGE = 2.0
endif

ifndef NO_SIGN_ASSEMBLY
sign = sign_assembly
else
sign = 
endif

all-local: $(the_lib)

install-local: $(the_lib) $(gacutil) $(sign)
	$(RUNTIME)  $(gacutil) /i $(the_lib) /f /root $(DESTDIR)$(prefix)/lib /package $(PACKAGE)

uninstall-local: $(gacutil)
	$(RUNTIME) $(gacutil) /u `echo $(LIBRARY_NAME) | sed 's,.dll$,,'`

$(gacutil):
	cd $(topdir)/tools/gacutil && $(MAKE)

$(sn):
	cd $(topdir)/tools/security && $(MAKE) sn.exe || exit 1 ;

sign_assembly: $(sn)
	$(RUNTIME) $(sn) -q -R $(the_lib) $(topdir)/class/mono.snk

clean-local:
	-rm -f $(the_lib) $(makefrag) $(test_lib) \
	       $(test_makefrag) $(test_response) \
	       $(the_pdb) $(test_pdb) $(CLEAN_FILES) \
	       TestResult.xml
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

# Fun with dependency tracking

$(the_lib): $(makefrag) $(response)
	$(LIBRARY_COMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) /target:library /out:$@ @$(response)

$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@sed 's,^,$(the_lib): ,' $< >$@

ifdef PLATFORM_CHANGE_SEPARATOR_CMD
$(response): $(sourcefile)
	@echo Creating $@ ...
	@$(PLATFORM_CHANGE_SEPARATOR_CMD) $(sourcefile) >$@
endif

-include $(makefrag)

# for now, don't give any /lib flags or set MONO_PATH, since we
# give a full path to the assembly.

ifdef HAVE_CS_TESTS

$(test_lib): $(test_makefrag) $(the_lib) $(test_response) $(test_nunit_dep)
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

$(btest_lib): $(btest_makefrag) $(the_lib) $(btest_response) $(test_nunit_dep)
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
