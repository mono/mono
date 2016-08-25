# -*- makefile -*-
#
# Rules for building unit tests.
# 
# Includers of this file must define the following values:
#
#   ASSEMBLY
#   ASSEMBLY_EXT
#   the_assembly

tests_CLEAN_FILES := 

ifndef TEST_COMPILE
TEST_COMPILE = $(subst $(test_remove),,$(CSCOMPILE))
endif

TEST_RUNTIME_WRAPPERS_PATH = $(shell dirname $(RUNTIME))/_tmpinst/bin

## Unit test support
ifndef NO_TEST

ifdef NUNIT_LITE
test_nunit_lib = nunitlite.dll
else
test_nunit_lib = nunit.framework.dll nunit.core.dll nunit.util.dll nunit.mocks.dll
endif

TEST_LIB_MCS_FLAGS = $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE)/%.dll,$(TEST_LIB_REFS))

test_nunit_dep = $(test_nunit_lib:%=$(topdir)/class/lib/$(PROFILE)/$(PARENT_PROFILE)%)
test_nunit_ref = $(test_nunit_dep:%=-r:%)
tests_CLEAN_FILES += TestResult*.xml

test_sourcefile = $(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_test.dll.sources)

ifeq ($(wildcard $(test_sourcefile)),)
test_sourcefile = $(ASSEMBLY:$(ASSEMBLY_EXT)=_test.dll.sources)
endif

test_lib = $(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_test.dll)
test_sourcefile_excludes = $(test_lib).exclude.sources

test_pdb = $(test_lib:.dll=.pdb)
test_response = $(depsdir)/$(test_lib).response
test_makefrag = $(depsdir)/$(test_lib).makefrag
test_flags = -r:$(the_assembly) $(test_nunit_ref) $(TEST_MCS_FLAGS) $(TEST_LIB_MCS_FLAGS)
tests_CLEAN_FILES += $(ASSEMBLY:$(ASSEMBLY_EXT)=_test*.dll) $(ASSEMBLY:$(ASSEMBLY_EXT)=_test*.pdb) $(test_response) $(test_makefrag)

ifndef HAVE_CS_TESTS
HAVE_CS_TESTS := $(wildcard $(test_sourcefile))
endif

HAVE_SOURCE_EXCLUDES := $(wildcard $(test_sourcefile_excludes))

endif # !NO_TEST

ifndef NO_TEST
$(test_nunit_dep): $(topdir)/build/deps/nunit-$(PROFILE).stamp
	@if test -f $@; then :; else rm -f $<; $(MAKE) $<; fi

ifdef NUNIT_LITE
$(topdir)/build/deps/nunit-$(PROFILE).stamp:
ifndef PARENT_PROFILE
	cd ${topdir}/tools/nunit-lite && $(MAKE)
endif
	echo "stamp" >$@
else
$(topdir)/build/deps/nunit-$(PROFILE).stamp:
ifndef PARENT_PROFILE
	cd ${topdir}/nunit24 && $(MAKE)
endif
	echo "stamp" >$@
endif

tests_CLEAN_FILES += $(topdir)/build/deps/nunit-$(PROFILE).stamp
endif

test_assemblies :=

ifdef HAVE_CS_TESTS
test_assemblies += $(test_lib)
endif

ifdef test_assemblies
check: run-test
test-local: $(test_assemblies)
run-test-local: run-test-lib
run-test-ondotnet-local: run-test-ondotnet-lib

TEST_HARNESS_EXCLUDES = -exclude=$(PLATFORM_TEST_HARNESS_EXCLUDES)$(PROFILE_TEST_HARNESS_EXCLUDES)NotWorking,ValueAdd,CAS,InetAccess
TEST_HARNESS_EXCLUDES_ONDOTNET = /exclude:$(PLATFORM_TEST_HARNESS_EXCLUDES)$(PROFILE_TEST_HARNESS_EXCLUDES)NotDotNet,CAS

ifdef NUNIT_LITE
NOSHADOW_FLAG =
NUNIT_XML_FLAG = -format:nunit2 -result:
OUTPUT_FILE_FLAG=-out
else
OUTPUT_FILE_FLAG=-output
NOSHADOW_FLAG = -noshadow
NUNIT_XML_FLAG = -xml=
endif

ifdef NUNIT_LITE
NOSHADOW_FLAG =
NUNIT_XML_FLAG = -format:nunit2 -result:
OUTPUT_FILE_FLAG=-out
else
OUTPUT_FILE_FLAG=-output
NOSHADOW_FLAG = -noshadow
NUNIT_XML_FLAG = -xml=
endif

ifdef TEST_HARNESS_VERBOSE
TEST_HARNESS_OUTPUT = -labels
TEST_HARNESS_OUTPUT_ONDOTNET = -labels
TEST_HARNESS_POSTPROC = :
TEST_HARNESS_POSTPROC_ONDOTNET = :
else
TEST_HARNESS_OUTPUT = $(OUTPUT_FILE_FLAG)=TestResult-$(PROFILE).log
TEST_HARNESS_OUTPUT_ONDOTNET = $(OUTPUT_FILE_FLAG)=TestResult-ondotnet-$(PROFILE).log
TEST_HARNESS_POSTPROC = (echo ''; cat TestResult-$(PROFILE).log) | sed '1,/^Tests run: /d'; xsltproc $(topdir)/build/nunit-summary.xsl TestResult-$(PROFILE).xml >> TestResult-$(PROFILE).log
TEST_HARNESS_POSTPROC_ONDOTNET = (echo ''; cat TestResult-ondotnet-$(PROFILE).log) | sed '1,/^Tests run: /d'; xsltproc $(topdir)/build/nunit-summary.xsl TestResult-ondotnet-$(PROFILE).xml >> TestResult-ondotnet-$(PROFILE).log
endif

ifdef FIXTURE
ifdef NUNIT_LITE
FIXTURE_ARG = -test=MonoTests.$(FIXTURE)
else
FIXTURE_ARG = -fixture=MonoTests.$(FIXTURE)
endif
endif

ifdef TESTNAME
ifdef NUNIT_LITE
TESTNAME_ARG = -test=MonoTests.$(TESTNAME)
else
TESTNAME_ARG = -run=MonoTests.$(TESTNAME)
endif
endif

ifdef ALWAYS_AOT
test-local-aot-compile: $(topdir)/build/deps/nunit-$(PROFILE).stamp
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" MONO_REGISTRY_PATH="$(HOME)/.mono/registry" MONO_TESTS_IN_PROGRESS="yes" $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(AOT_BUILD_FLAGS) $(test_assemblies)

else
test-local-aot-compile: $(topdir)/build/deps/nunit-$(PROFILE).stamp

endif # ALWAYS_AOT

## FIXME: i18n problem in the 'sed' command below
run-test-lib: test-local test-local-aot-compile
	ok=:; \
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" MONO_REGISTRY_PATH="$(HOME)/.mono/registry" MONO_TESTS_IN_PROGRESS="yes" $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(AOT_RUN_FLAGS) $(TEST_HARNESS) $(test_assemblies) $(NOSHADOW_FLAG) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) $(TEST_HARNESS_EXCLUDES) $(TEST_HARNESS_OUTPUT) $(NUNIT_XML_FLAG)TestResult-$(PROFILE).xml $(FIXTURE_ARG) $(TESTNAME_ARG)|| ok=false; \
	if [ ! -f "TestResult-$(PROFILE).xml" ]; then echo "<?xml version='1.0' encoding='utf-8'?><test-results failures='1' total='1' not-run='0' name='bcl-tests' date='$$(date +%F)' time='$$(date +%T)'><test-suite name='$(strip $(test_assemblies))' success='False' time='0'><results><test-case name='crash' executed='True' success='False' time='0'><failure><message>The test runner didn't produce a test result XML, probably due to a crash of the runtime. Check the log for more details.</message><stack-trace></stack-trace></failure></test-case></results></test-suite></test-results>" > TestResult-$(PROFILE).xml; fi; \
	$(TEST_HARNESS_POSTPROC) ; $$ok

## Instructs compiler to compile to target .net execution, it can be usefull in rare cases when runtime detection is not possible
run-test-ondotnet-lib: LOCAL_TEST_COMPILER_ONDOTNET_FLAGS:=-d:RUN_ONDOTNET
run-test-ondotnet-lib: test-local
	ok=:; \
	$(TEST_HARNESS) $(test_assemblies) $(NOSHADOW_FLAG) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_ONDOTNET_FLAGS) $(TEST_HARNESS_EXCLUDES_ONDOTNET) $(TEST_HARNESS_OUTPUT_ONDOTNET) $(NUNIT_XML_FLAG)=TestResult-ondotnet-$(PROFILE).xml $(FIXTURE_ARG) $(TESTNAME_ARG) || ok=false; \
	$(TEST_HARNESS_POSTPROC_ONDOTNET) ; $$ok


endif # test_assemblies

TEST_FILES =

ifdef HAVE_CS_TESTS
TEST_FILES += `sed -e '/^$$/d' -e 's,^../,,' -et -e 's,^,Test/,' $(test_sourcefile)`
endif

ifdef HAVE_CS_TESTS

$(test_lib): $(the_assembly) $(test_response) $(test_nunit_dep)
	$(TEST_COMPILE) $(LIBRARY_FLAGS) -target:library -out:$@ $(test_flags) $(LOCAL_TEST_COMPILER_ONDOTNET_FLAGS) @$(test_response)

test_response_preprocessed = $(test_response)_preprocessed

# This handles .excludes/.sources pairs, as well as resolving the
# includes that occur in .sources files
$(test_response_preprocessed): $(test_sourcefile)
	$(SHELL) $(topdir)/build/gensources.sh $@ '$(test_sourcefile)' '$(test_sourcefile_excludes)'

$(test_response): $(test_response_preprocessed)
#	@echo Creating $@ ...
	@sed -e '/^$$/d' -e 's,^,Test/,' $(test_response_preprocessed) | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

$(test_makefrag): $(test_response)
#	@echo Creating $@ ...
	@sed 's,^,$(test_lib): ,' $< >$@

-include $(test_makefrag)

endif

