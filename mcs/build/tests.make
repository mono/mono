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

test_nunit_lib = nunitlite.dll
xunit_core := xunit.core xunit.execution.desktop xunit.abstractions xunit.assert Xunit.NetCore.Extensions
xunit_deps := System.Runtime
xunit_src  := $(patsubst %,$(topdir)/../external/xunit-binaries/%,BenchmarkAttribute.cs BenchmarkDiscover.cs) $(topdir)/../mcs/class/test-helpers/PlatformDetection.cs

ifeq ($(USE_XTEST_REMOTE_EXECUTOR), YES)
XTEST_REMOTE_EXECUTOR = $(xunit_test_lib)_RemoteExecuter.exe
xunit_src += $(topdir)/../mcs/class/test-helpers/AdminHelper.cs $(topdir)/../mcs/class/test-helpers/RemoteExecutorTestBase.Mono.cs $(topdir)/../external/corefx/src/CoreFx.Private.TestUtilities/src/System/IO/FileCleanupTestBase.cs $(topdir)/../external/corefx/src/CoreFx.Private.TestUtilities/src/System/Diagnostics/RemoteExecutorTestBase.Process.cs $(topdir)/../external/corefx/src/CoreFx.Private.TestUtilities/src/System/Diagnostics/RemoteExecutorTestBase.cs $(topdir)/../external/corefx/src/Common/src/System/PasteArguments.cs
endif

xunit_class_deps := 

xunit_libs_ref = $(patsubst %,-r:$(topdir)/../external/xunit-binaries/%.dll,$(xunit_core))
xunit_libs_ref += $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE)/Facades/%.dll,$(xunit_deps))

xunit_libs_dep = $(xunit_class_deps:%=$(topdir)/class/lib/$(PROFILE)/$(PARENT_PROFILE)%.dll)
xunit_libs_ref += $(xunit_libs_dep:%=-r:%)

TEST_LIB_MCS_FLAGS = $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE)/%.dll,$(TEST_LIB_REFS))
XTEST_LIB_MCS_FLAGS = $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE)/%.dll,$(XTEST_LIB_REFS))

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
test_flags = $(test_nunit_ref) $(TEST_MCS_FLAGS) $(TEST_LIB_MCS_FLAGS)
ifndef NO_BUILD
test_flags += -r:$(the_assembly)
test_assembly_dep = $(the_assembly)
endif
tests_CLEAN_FILES += $(ASSEMBLY:$(ASSEMBLY_EXT)=_test*.dll) $(ASSEMBLY:$(ASSEMBLY_EXT)=_test*.pdb) $(test_response) $(test_makefrag)

xtest_sourcefile = $(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_xtest.dll.sources)
xtest_sourcefile_excludes = $(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_xtest.dll.exclude.sources)

xunit_test_lib = $(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_xunit-test.dll)

xtest_response = $(depsdir)/$(xunit_test_lib).response
xtest_makefrag = $(depsdir)/$(xunit_test_lib).makefrag
xtest_flags = -r:$(the_assembly) $(xunit_libs_ref) $(XTEST_MCS_FLAGS) $(XTEST_LIB_MCS_FLAGS) /unsafe

ifeq ($(wildcard $(xtest_sourcefile)),)
xtest_sourcefile = $(ASSEMBLY:$(ASSEMBLY_EXT)=_xtest.dll.sources)
tests_CLEAN_FILES += $(xunit_test_lib) $(xtest_response) $(xtest_makefrag)
endif

ifndef HAVE_CS_TESTS
HAVE_CS_TESTS := $(wildcard $(test_sourcefile))
endif

HAVE_CS_XTESTS := $(wildcard $(xtest_sourcefile))

endif # !NO_TEST

ifndef NO_TEST
$(test_nunit_dep): $(topdir)/build/deps/nunit-$(PROFILE).stamp
	@if test -f $@; then :; else rm -f $<; $(MAKE) $<; fi

$(topdir)/build/deps/nunit-$(PROFILE).stamp:
ifndef PARENT_PROFILE
	cd ${topdir}/tools/nunit-lite && $(MAKE)
endif
	echo "stamp" >$@

tests_CLEAN_FILES += $(topdir)/build/deps/nunit-$(PROFILE).stamp

endif

test_assemblies :=

test_lib_dir = $(topdir)/class/lib/$(PROFILE)/tests

test_lib_output = $(topdir)/class/lib/$(PROFILE)/tests/$(test_lib)

ifdef HAVE_CS_TESTS
test_assemblies += $(test_lib_output)

check: run-test
test-local: $(test_assemblies)
run-test-local: run-test-lib
run-test-ondotnet-local: run-test-ondotnet-lib

ifdef TEST_WITH_INTERPRETER
TEST_HARNESS_EXCLUDES = -exclude=$(PLATFORM_TEST_HARNESS_EXCLUDES)$(PROFILE_TEST_HARNESS_EXCLUDES)NotWorking,NotWorkingRuntimeInterpreter,CAS
else
TEST_HARNESS_EXCLUDES = -exclude=$(PLATFORM_TEST_HARNESS_EXCLUDES)$(PROFILE_TEST_HARNESS_EXCLUDES)NotWorking,CAS
endif

TEST_HARNESS_EXCLUDES_ONDOTNET = /exclude:$(PLATFORM_TEST_HARNESS_EXCLUDES)$(PROFILE_TEST_HARNESS_EXCLUDES)NotDotNet,CAS

NOSHADOW_FLAG =

ifdef FIXTURE
FIXTURE_ARG = -test=MonoTests.$(FIXTURE)
endif

ifdef TESTNAME
TESTNAME_ARG = -test=MonoTests.$(TESTNAME)
endif

ifdef TEST_HARNESS_VERBOSE
LABELS_ARG = -labels
endif

ifdef ALWAYS_AOT
test-local-aot-compile: $(topdir)/build/deps/nunit-$(PROFILE).stamp $(test_assemblies)
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" MONO_REGISTRY_PATH="$(HOME)/.mono/registry" MONO_TESTS_IN_PROGRESS="yes" $(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(AOT_BUILD_FLAGS) $(test_assemblies)

else
test-local-aot-compile: $(topdir)/build/deps/nunit-$(PROFILE).stamp $(test_assemblies)

endif # ALWAYS_AOT

ifdef COVERAGE
TEST_COVERAGE_FLAGS = -O=-aot --profile=coverage:output=$(topdir)/class/lib/$(PROFILE_DIRECTORY)/coverage_nunit_$(ASSEMBLY).xml
endif

NUNITLITE_CONFIG_FILE=$(topdir)/class/lib/$(PROFILE)/$(PARENT_PROFILE)nunit-lite-console.exe.config

patch-nunitlite-appconfig:
	cp -f $(topdir)/tools/nunit-lite/nunit-lite-console/nunit-lite-console.exe.config.tmpl $(NUNITLITE_CONFIG_FILE)
ifdef TEST_NUNITLITE_APP_CONFIG_GLOBAL
	sed -i -e "/__INSERT_CUSTOM_APP_CONFIG_GLOBAL__/r $(TEST_NUNITLITE_APP_CONFIG_GLOBAL)" $(NUNITLITE_CONFIG_FILE)
endif
ifdef TEST_NUNITLITE_APP_CONFIG_RUNTIME
	sed -i -e "/__INSERT_CUSTOM_APP_CONFIG_RUNTIME__/r $(TEST_NUNITLITE_APP_CONFIG_RUNTIME)" $(NUNITLITE_CONFIG_FILE)
endif

ifdef PLATFORM_AOT_SUFFIX

DEDUP_DUMMY_CS=$(topdir)/class/lib/$(PROFILE)/DedupInflatedMethods.cs
DEDUP_DUMMY=$(topdir)/class/lib/$(PROFILE)/DedupInflatedMethods.dll

$(DEDUP_DUMMY):
	echo " // Empty Assembly \n\n" > $(DEDUP_DUMMY_CS)
	$(CSCOMPILE) -t:library -out:$(DEDUP_DUMMY) $(DEDUP_DUMMY_CS) 
	rm $(DEDUP_DUMMY_CS)

MKBUNDLE_TEST_BIN = $(TEST_HARNESS).static
MKBUNDLE_EXE = $(topdir)/class/lib/$(PROFILE)/mkbundle.exe
# Pattern based on the one in AOT_PROFILE_ASSEMBLIES 
# It's easier if you read it backwards.
# What we do here is get the files in the profile directory that end in "test.dll" or are prefixed with nunit (filter)
# and then strip out everything that we expect to live outside the top level (filter-out)
TEST_ASSEMBLIES:=$(sort $(patsubst .//%,%,$(filter-out %.exe.static %.dll.dll %.exe.dll %bare% %plaincore% %secxml% %Facades% %ilasm%,$(filter %.dll,$(wildcard $(topdir)/class/lib/$(PROFILE)/tests/*)))))

$(MKBUNDLE_EXE): $(topdir)/tools/mkbundle/mkbundle.cs
	make -C $(topdir)/tools/mkbundle

mkbundle-all-tests:
	$(Q_AOT) $(MAKE) -C $(topdir)/class do-test
	$(Q_AOT) $(MAKE) -C $(topdir)/tools/mkbundle
	$(Q_AOT) $(MAKE) $(MKBUNDLE_TEST_BIN) # recursive make re-computes variables for TEST_ASSEMBLIES

ifdef MKBUNDLE_DEDUP
MKBUNDLE_DEDUP_COND := $(DEDUP_DUMMY)
DEDUP_ARGS=--aot-dedup $(DEDUP_DUMMY)
endif

$(MKBUNDLE_TEST_BIN): $(TEST_ASSEMBLIES) $(TEST_HARNESS) $(MKBUNDLE_EXE) $(MKBUNDLE_DEDUP_COND)
	$(Q_AOT) MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)" PKG_CONFIG_PATH="$(topdir)/../data" $(RUNTIME) $(RUNTIME_FLAGS) $(MKBUNDLE_EXE) -L $(topdir)/class/lib/$(PROFILE) -v --deps $(TEST_HARNESS) $(TEST_ASSEMBLIES) -o $(MKBUNDLE_TEST_BIN) --aot-mode $(AOT_MODE) --aot-runtime $(RUNTIME) --aot-args $(AOT_BUILD_ATTRS) --in-tree $(topdir)/.. --managed-linker $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/monolinker.exe --config $(topdir)/../data/config --i18n all $(DEDUP_ARGS) --keeptemp

endif # PLATFORM_AOT_SUFFIX

ifneq ($(wildcard $(MKBUNDLE_TEST_BIN)),)
TEST_HARNESS_EXEC=$(MKBUNDLE_TEST_BIN)
TEST_HARNESS_EXCLUDES:=$(TEST_HARNESS_EXCLUDES),StaticLinkedAotNotWorking
else 
TEST_HARNESS_EXEC=$(TEST_RUNTIME) $(RUNTIME_FLAGS) $(TEST_COVERAGE_FLAGS) $(AOT_RUN_FLAGS) $(TEST_HARNESS) 
endif

## FIXME: i18n problem in the 'sed' command below
run-test-lib: test-local test-local-aot-compile patch-nunitlite-appconfig
	ok=:; \
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" MONO_REGISTRY_PATH="$(HOME)/.mono/registry" MONO_TESTS_IN_PROGRESS="yes" $(TEST_HARNESS_EXEC) $(test_assemblies) $(NOSHADOW_FLAG) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) $(TEST_HARNESS_EXCLUDES) $(LABELS_ARG) -format:nunit2 -result:TestResult-$(PROFILE).xml $(FIXTURE_ARG) $(TESTNAME_ARG)|| ok=false; \
	if [ ! -f "TestResult-$(PROFILE).xml" ]; then echo "<?xml version='1.0' encoding='utf-8'?><test-results failures='1' total='1' not-run='0' name='bcl-tests' date='$$(date +%F)' time='$$(date +%T)'><test-suite name='$(strip $(test_assemblies))' success='False' time='0'><results><test-case name='crash' executed='True' success='False' time='0'><failure><message>The test runner didn't produce a test result XML, probably due to a crash of the runtime. Check the log for more details.</message><stack-trace></stack-trace></failure></test-case></results></test-suite></test-results>" > TestResult-$(PROFILE).xml; fi; \
	$$ok

## Instructs compiler to compile to target .net execution, it can be usefull in rare cases when runtime detection is not possible
run-test-ondotnet-lib: LOCAL_TEST_COMPILER_ONDOTNET_FLAGS:=-d:RUN_ONDOTNET
run-test-ondotnet-lib: test-local
	ok=:; \
	$(TEST_HARNESS) $(test_assemblies) $(NOSHADOW_FLAG) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_ONDOTNET_FLAGS) $(TEST_HARNESS_EXCLUDES_ONDOTNET) $(LABELS_ARG) -format:nunit2 -result:TestResult-ondotnet-$(PROFILE).xml $(FIXTURE_ARG) $(TESTNAME_ARG) || ok=false; \
	$$ok


endif

TEST_FILES =

ifdef HAVE_CS_TESTS
TEST_FILES += `sed -e '/^$$/d' -e 's,^../,,' -e '/^\#.*$$/d' -et -e 's,^,Test/,' $(test_sourcefile)`
endif

ifdef HAVE_CS_TESTS

$(test_lib_dir):
	mkdir -p $@

$(test_lib_output): $(test_assembly_dep) $(test_response) $(test_nunit_dep) $(test_lib_dir)
	$(TEST_COMPILE) $(LIBRARY_FLAGS) -target:library -out:$@ $(test_flags) $(LOCAL_TEST_COMPILER_ONDOTNET_FLAGS) @$(test_response)

test_response_preprocessed = $(test_response)_preprocessed

# This handles .excludes/.sources pairs, as well as resolving the
# includes that occur in .sources files
$(test_response_preprocessed): $(test_sourcefile) $(wildcard $(test_sourcefile_excludes))
	$(SHELL) $(topdir)/build/gensources.sh $@ '$(test_sourcefile)' '$(test_sourcefile_excludes)'

$(test_response): $(test_response_preprocessed)
#	@echo Creating $@ ...
	@sed -e '/^$$/d' -e 's,^,Test/,' $(test_response_preprocessed) | $(PLATFORM_CHANGE_SEPARATOR_CMD) >$@

$(test_makefrag): $(test_response)
#	@echo Creating $@ ...
	@sed 's,^,$(test_lib_output): ,' $< >$@

-include $(test_makefrag)

build-test-lib: $(test_lib_output)
	@echo Building testing lib

endif


ifdef HAVE_CS_XTESTS

XTEST_HARNESS_PATH = $(topdir)/../external/xunit-binaries
XTEST_HARNESS = $(XTEST_HARNESS_PATH)/xunit.console.exe
XTEST_HARNESS_FLAGS := -noappdomain -noshadow -parallel none -nunit TestResult-$(PROFILE)-xunit.xml
XTEST_TRAIT := -notrait category=failing -notrait category=nonmonotests -notrait Benchmark=true -notrait category=outerloop

ifdef FIXTURE
XTEST_HARNESS_FLAGS += -class $(FIXTURE)
endif

ifdef TESTNAME
XTEST_HARNESS_FLAGS += -method $(TESTNAME)
endif

ifdef COVERAGE
XTEST_COVERAGE_FLAGS = -O=-aot --profile=coverage:output=$(topdir)/class/lib/$(PROFILE_DIRECTORY)/coverage_xunit_$(ASSEMBLY).xml
endif

check: run-xunit-test-local
xunit-test-local: $(xunit_test_lib)
run-xunit-test-local: run-xunit-test-lib

# cp -rf is a HACK for xunit runner to require xunit.execution.desktop.dll file in local folder on .net only
run-xunit-test-lib: xunit-test-local $(XTEST_REMOTE_EXECUTOR)
	@cp -rf $(XTEST_HARNESS_PATH)/xunit.execution.desktop.dll xunit.execution.desktop.dll
	ok=:; \
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" $(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(XTEST_COVERAGE_FLAGS) $(AOT_RUN_FLAGS) $(XTEST_HARNESS) $(xunit_test_lib) $(XTEST_HARNESS_FLAGS) $(XTEST_TRAIT) || ok=false; \
	$$ok
	@rm -f xunit.execution.desktop.dll

# Some xunit tests want to be executed in a separate process (see RemoteExecutorTestBase)
$(XTEST_REMOTE_EXECUTOR): $(topdir)/../mcs/class/test-helpers/RemoteExecutorConsoleApp.cs
	$(TEST_COMPILE) $(topdir)/../mcs/class/test-helpers/RemoteExecutorConsoleApp.cs -r:$(xunit_test_lib) $(xtest_flags) /debug-

$(xunit_test_lib): $(the_assembly) $(xtest_response) $(xunit_libs_dep) $(xunit_src)
	$(TEST_COMPILE) $(LIBRARY_FLAGS) $(XTEST_LIB_FLAGS) -target:library -out:$@ $(xtest_flags) @$(xtest_response) $(xunit_src)

xtest_response_preprocessed = $(xtest_response)_preprocessed

# This handles .excludes/.sources pairs, as well as resolving the
# includes that occur in .sources files
$(xtest_response): $(xtest_sourcefile) $(wildcard $(xtest_sourcefile_excludes))
	$(SHELL) $(topdir)/build/gensources.sh $@ '$(xtest_sourcefile)' '$(xtest_sourcefile_excludes)'

$(xtest_makefrag): $(xtest_response)
	@echo Creating $@ ...
	@sed 's,^,$(xunit_test_lib): ,' $< >$@

-include $(xtest_makefrag)

endif


.PHONY: patch-nunitlite-appconfig
