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
library_CLEAN_FILES += $(response) $(LIBRARY).mdb $(BUILT_SOURCES)
endif

ifndef LIBRARY_NAME
LIBRARY_NAME = $(LIBRARY)
endif

makefrag = $(depsdir)/$(PROFILE)_$(LIBRARY).makefrag
the_lib = $(topdir)/class/lib/$(PROFILE)/$(LIBRARY_NAME)
the_pdb = $(the_lib:.dll=.pdb)
the_mdb = $(the_lib).mdb
library_CLEAN_FILES += $(makefrag) $(the_lib) $(the_pdb) $(the_mdb)

ifndef NO_TEST
test_nunit_lib = nunit.framework.dll nunit.core.dll nunit.util.dll
test_nunit_dep = $(test_nunit_lib:%=$(topdir)/class/lib/$(PROFILE)/%)
test_nunit_ref = $(test_nunit_dep:%=-r:%)
library_CLEAN_FILES += TestResult*.xml

ifndef test_against
test_against = $(the_lib)
test_dep = $(the_lib)
endif

ifndef test_lib
test_lib = $(LIBRARY:.dll=_test_$(PROFILE).dll)
test_sourcefile = $(LIBRARY:.dll=_test.dll.sources)
else
test_sourcefile = $(test_lib).sources
endif
test_pdb = $(test_lib:.dll=.pdb)
test_response = $(depsdir)/$(test_lib).response
test_makefrag = $(depsdir)/$(test_lib).makefrag
test_flags = /r:$(test_against) $(test_nunit_ref) $(TEST_MCS_FLAGS)
library_CLEAN_FILES += $(LIBRARY:.dll=_test*.dll) $(LIBRARY:.dll=_test*.pdb) $(test_response) $(test_makefrag)

ifndef btest_lib
btest_lib = $(LIBRARY:.dll=_btest_$(PROFILE).dll)
btest_sourcefile = $(LIBRARY:.dll=_btest.dll.sources)
else
btest_sourcefile = $(btest_lib).sources
endif
btest_pdb = $(btest_lib:.dll=.pdb)
btest_response = $(depsdir)/$(btest_lib).response
btest_makefrag = $(depsdir)/$(btest_lib).makefrag
btest_flags = /r:$(test_against) $(test_nunit_ref) $(TEST_MBAS_FLAGS)
library_CLEAN_FILES += $(LIBRARY:.dll=_btest*.dll) $(LIBRARY:.dll=_btest*.pdb) $(btest_response) $(btest_makefrag)

ifndef HAVE_CS_TESTS
HAVE_CS_TESTS := $(wildcard $(test_sourcefile))
endif
ifndef HAVE_VB_TESTS
HAVE_VB_TESTS := $(wildcard $(btest_sourcefile))
endif

endif

gacutil = $(topdir)/tools/gacutil/gacutil.exe
GACUTIL = MONO_PATH="$(topdir)/class/lib/default:$$MONO_PATH" $(RUNTIME) $(gacutil)

ifdef NO_SIGN_ASSEMBLY
SN = :
else
sn = $(topdir)/class/lib/net_1_1_bootstrap/sn.exe
SN = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap:$$MONO_PATH" $(RUNTIME) $(sn)
SNFLAGS = -q -R
endif

PACKAGE = 1.0

ifeq ($(PROFILE), net_2_0)
PACKAGE = 2.0
endif

libdir = $(prefix)/lib

ifeq ($(PLATFORM), win32)
GACDIR = `cygpath -w $(libdir)`
GACROOT = `cygpath -w $(DESTDIR)$(libdir)`
else
GACDIR = $(libdir)
GACROOT = $(DESTDIR)$(libdir)
endif

all-local install-local test-local: $(the_lib)

ifdef NO_INSTALL
install-local uninstall-local:
	@:

else

ifdef LIBRARY_INSTALL_DIR
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(the_lib) $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME)
	-$(INSTALL_LIB) $(the_lib).mdb $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME).mdb

uninstall-local:
	-rm -f $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME) $(DESTDIR)$(LIBRARY_INSTALL_DIR)/$(LIBRARY_NAME).mdb

else

install-local: $(gacutil)
	$(GACUTIL) /i $(the_lib) /f /gacdir $(GACDIR) /root $(GACROOT) /package $(PACKAGE)

uninstall-local: $(gacutil)
	$(GACUTIL) /u $(LIBRARY_NAME:.dll=)

endif
endif

clean-local:
	-rm -f $(library_CLEAN_FILES) $(CLEAN_FILES)

test-local run-test-local run-test-ondotnet-local:
	@:

ifndef NO_TEST
$(test_nunit_dep): $(topdir)/build/deps/nunit-$(PROFILE).stamp
	@if test -f $@; then :; else rm -f $<; $(MAKE) $<; fi
$(topdir)/build/deps/nunit-$(PROFILE).stamp:
	cd ${topdir}/nunit20 && $(MAKE)
	echo "stamp" >$@
library_CLEAN_FILES += $(topdir)/build/deps/nunit-$(PROFILE).stamp
endif

test_assemblies :=

ifdef HAVE_CS_TESTS
test_assemblies += $(test_lib)
endif

ifdef HAVE_VB_TESTS
test_assemblies += $(btest_lib)
endif

ifdef test_assemblies
test-local: $(test_assemblies)
run-test-local: run-test-lib
run-test-ondotnet-local: run-test-ondotnet-lib

run-test-lib: test-local
	$(TEST_RUNTIME) $(TEST_HARNESS) $(TEST_HARNESS_FLAGS) /xml:TestResult-$(PROFILE).xml $(test_assemblies)

run-test-ondotnet-lib: test-local
	$(TEST_HARNESS) $(TEST_HARNESS_FLAGS) /xml:TestResult-ondotnet-$(PROFILE).xml $(test_assemblies)
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
	  case $$f in \
	  ../*) : ;; \
	  *) dest=`dirname $(distdir)/$$f` ; $(MKINSTALLDIRS) $$dest && cp -p $$f $$dest || exit 1 ;; \
	  esac ; done

ifndef LIBRARY_COMPILE
LIBRARY_COMPILE = $(CSCOMPILE)
endif

ifndef TEST_COMPILE
TEST_COMPILE = $(CSCOMPILE)
endif

ifndef BTEST_COMPILE
BTEST_COMPILE = $(BASCOMPILE)
endif

ifndef LIBRARY_SNK
LIBRARY_SNK = $(topdir)/class/mono.snk
endif

$(gacutil):
	cd $(topdir)/tools/gacutil && $(MAKE) PROFILE=default

ifdef sn
$(sn):
	cd $(topdir) && $(MAKE) PROFILE=net_1_1_bootstrap
endif

# The library

$(the_lib): $(response) $(sn) $(BUILT_SOURCES)
ifdef LIBRARY_USE_INTERMEDIATE_FILE
	$(LIBRARY_COMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) /target:library /out:$(@F) $(BUILT_SOURCES) @$(response)
	$(SN) $(SNFLAGS) $(@F) $(LIBRARY_SNK)
	mv $(@F) $@
	-mv $(@F).mdb $@.mdb
else
	$(LIBRARY_COMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) /target:library /out:$@ $(BUILT_SOURCES) @$(response)
	$(SN) $(SNFLAGS) $@ $(LIBRARY_SNK)
endif

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

$(test_lib): $(test_dep) $(test_response) $(test_nunit_dep)
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

$(btest_lib): $(test_dep) $(btest_response) $(test_nunit_dep)
	$(BTEST_COMPILE) /target:library /out:$@ $(btest_flags) @$(btest_response)

$(btest_response): $(btest_sourcefile)
	@echo Creating $@ ...
	@sed 's,^,Test/,' $(btest_sourcefile) | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

$(btest_makefrag): $(btest_response)
	@echo Creating $@ ...
	@sed 's,^,$(btest_lib): ,' $< >$@

-include $(btest_makefrag)

endif

all-local: $(makefrag) $(test_makefrag) $(btest_makefrag)
$(makefrag) $(test_makefrag) $(btest_makefrag): $(topdir)/build/library.make
