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

test_lib_dir = $(topdir)/class/lib/$(PROFILE)/tests

test_nunit_lib = nunitlite.dll
xunit_core := xunit.core xunit.execution.dotnet xunit.abstractions xunit.assert Xunit.NetCore.Extensions
xunit_deps := netstandard System.Runtime
xunit_src  := $(patsubst %,$(topdir)/../external/xunit-binaries/%,BenchmarkAttribute.cs BenchmarkDiscover.cs) $(topdir)/../mcs/class/test-helpers/PlatformDetection.cs

ifeq ($(USE_XTEST_REMOTE_EXECUTOR), YES)
XTEST_REMOTE_EXECUTOR = $(test_lib_dir)/RemoteExecutorConsoleApp.exe

ifeq ($(BUILD_PLATFORM), win32)
XTEST_REMOTE_EXECUTOR_ABSPATH = $(shell cygpath -w $(abspath $(XTEST_REMOTE_EXECUTOR)))
else
XTEST_REMOTE_EXECUTOR_ABSPATH = $(abspath $(XTEST_REMOTE_EXECUTOR))
endif

xunit_src += $(topdir)/../mcs/class/test-helpers/AdminHelper.cs \
$(topdir)/../external/corefx/src/CoreFx.Private.TestUtilities/src/System/IO/FileCleanupTestBase.cs \
$(topdir)/../external/corefx/src/CoreFx.Private.TestUtilities/src/System/Diagnostics/RemoteExecutorTestBase.cs \
$(topdir)/../external/corefx/src/Common/src/System/PasteArguments.cs \
$(topdir)/../external/corefx/src/Common/src/System/PasteArguments.Unix.cs

ifdef MOBILE_PROFILE
xunit_src += $(topdir)/../mcs/class/test-helpers/RemoteExecutorTestBase.Mobile.cs
else
ifeq ($(PROFILE), xammac_net_4_5)
xunit_src += $(topdir)/../mcs/class/test-helpers/RemoteExecutorTestBase.Mobile.cs
else
xunit_src += $(topdir)/../mcs/class/test-helpers/RemoteExecutorTestBase.Mono.cs $(topdir)/../external/corefx/src/CoreFx.Private.TestUtilities/src/System/Diagnostics/RemoteExecutorTestBase.Process.cs
endif
endif

endif # ($(USE_XTEST_REMOTE_EXECUTOR), YES)

xunit_class_deps := 

xunit_libs_ref = $(patsubst %,-r:$(topdir)/../external/xunit-binaries/%.dll,$(xunit_core))
xunit_libs_ref += $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE)/Facades/%.dll,$(xunit_deps))

xunit_libs_dep = $(xunit_class_deps:%=$(topdir)/class/lib/$(PROFILE)/%.dll)
xunit_libs_ref += $(xunit_libs_dep:%=-r:%)

TEST_LIB_REFS_ALL = $(TEST_LIB_REFS) $(DEFAULT_REFERENCES)

ifdef TARGET_NET_REFERENCE
# System*, mscorlib references come from the TARGET_NET_REFERENCE dir, others from the profile dir
TEST_LIB_REFS_MONO = $(call _FILTER_OUT,System,$(call _FILTER_OUT,mscorlib,$(TEST_LIB_REFS_ALL)))
TEST_LIB_REFS_SYSTEM = $(filter-out $(TEST_LIB_REFS_MONO),$(TEST_LIB_REFS_ALL))

TEST_LIB_MCS_FLAGS = $(patsubst %,-r:$(topdir)/../external/binary-reference-assemblies/$(TARGET_NET_REFERENCE)/%.dll,$(TEST_LIB_REFS_SYSTEM))
TEST_LIB_MCS_FLAGS += $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/%.dll,$(TEST_LIB_REFS_MONO))
else
TEST_LIB_MCS_FLAGS = $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/%.dll,$(TEST_LIB_REFS_ALL))
endif

XTEST_LIB_MCS_FLAGS = $(patsubst %,-r:$(topdir)/class/lib/$(PROFILE)/%.dll,$(XTEST_LIB_REFS) $(DEFAULT_REFERENCES))

test_nunit_dep = $(test_nunit_lib:%=$(topdir)/class/lib/$(PROFILE)/%)
test_nunit_ref = $(test_nunit_dep:%=-r:%)
tests_CLEAN_FILES += TestResult*.xml

test_sourcefile_base = $(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_test.dll.sources)

ifeq ($(wildcard $(test_sourcefile_base)),)
test_sourcefile_base = $(ASSEMBLY:$(ASSEMBLY_EXT)=_test.dll.sources)
endif

test_lib = $(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_test.dll)
test_lib_output = $(test_lib_dir)/$(test_lib)

test_sourcefile_base_excludes = $(test_lib).exclude.sources

test_pdb = $(test_lib:.dll=.pdb)
test_flags = $(test_nunit_ref) $(TEST_MCS_FLAGS) $(TEST_LIB_MCS_FLAGS)
ifndef NO_BUILD
test_flags += -r:$(the_assembly)
test_assembly_dep = $(the_assembly)
endif
tests_CLEAN_FILES += $(test_lib_output) $(test_lib_output:$(ASSEMBLY_EXT)=.pdb)

xtest_sourcefile_base = $(PROFILE_PLATFORM)_$(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_xtest.dll.sources)
xtest_sourcefile_base_excludes_full = $(PROFILE_PLATFORM)_$(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_xtest.dll.exclude.sources)
xtest_sourcefile_base_excludes = $(xtest_sourcefile_base_excludes_full:_%=%)

xunit_test_lib = $(PROFILE)_$(ASSEMBLY:$(ASSEMBLY_EXT)=_xunit-test.dll)
xtest_lib_output = $(test_lib_dir)/$(xunit_test_lib)

xtest_response = $(depsdir)/$(xunit_test_lib).response
xtest_makefrag = $(depsdir)/$(xunit_test_lib).makefrag
xtest_flags = -r:$(the_assembly) $(xunit_libs_ref) $(XTEST_MCS_FLAGS) $(XTEST_LIB_MCS_FLAGS) /unsafe

ifeq ($(wildcard $(xtest_sourcefile_base)),)
xtest_sourcefile_base = $(ASSEMBLY:$(ASSEMBLY_EXT)=_xtest.dll.sources)
tests_CLEAN_FILES += $(xtest_lib_output) $(xtest_response) $(xtest_makefrag)
endif

ifndef HAVE_CS_TESTS
HAVE_CS_TESTS := $(wildcard $(test_sourcefile_base))
endif

HAVE_CS_XTESTS := $(wildcard $(xtest_sourcefile_base))

endif # !NO_TEST

ifndef NO_TEST
$(test_nunit_dep): $(topdir)/build/deps/nunit-$(PROFILE).stamp
	@if test -f $@; then :; else rm -f $<; $(MAKE) $<; fi

$(topdir)/build/deps/nunit-$(PROFILE).stamp:
	$(MAKE) -C ${topdir}/tools/nunit-lite
	echo "stamp" >$@

tests_CLEAN_FILES += $(topdir)/build/deps/nunit-$(PROFILE).stamp

endif

test_assemblies :=

ifdef HAVE_CS_TESTS
test_assemblies += $(test_lib_output)

check: run-test
test-local: $(test_assemblies) $(test_lib_dir)/nunit-excludes.txt
run-test-local: run-test-lib
run-test-ondotnet-local: run-test-ondotnet-lib

TEST_HARNESS_EXCLUDES = $(PLATFORM_TEST_HARNESS_EXCLUDES) $(PROFILE_TEST_HARNESS_EXCLUDES) NotWorking CAS

ifdef TEST_WITH_INTERPRETER
TEST_HARNESS_EXCLUDES += NotWorkingRuntimeInterpreter
endif

NOSHADOW_FLAG =

ifdef FIXTURE
FIXTURE_ARG = -test=$(FIXTURE)
endif

ifdef TESTNAME
TESTNAME_ARG = -test=$(TESTNAME)
endif

ifdef TEST_HARNESS_VERBOSE
LABELS_ARG = -labels
endif

ifdef ALWAYS_AOT_TESTS
test-local-aot-compile: $(topdir)/build/deps/nunit-$(PROFILE).stamp $(test_assemblies)
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" MONO_REGISTRY_PATH="$(HOME)/.mono/registry" MONO_TESTS_IN_PROGRESS="yes" $(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(AOT_BUILD_FLAGS) $(test_assemblies)

else
test-local-aot-compile: $(topdir)/build/deps/nunit-$(PROFILE).stamp $(test_assemblies)

endif # ALWAYS_AOT_TESTS

ifdef COVERAGE
TEST_COVERAGE_FLAGS = -O=-aot --profile=coverage:output=$(topdir)/class/lib/$(PROFILE_DIRECTORY)/coverage_nunit_$(ASSEMBLY).xml
endif

NUNITLITE_CONFIG_FILE=$(topdir)/class/lib/$(PROFILE)/$(PARENT_PROFILE)nunit-lite-console.exe.config

$(test_lib_output).nunitlite.config: $(topdir)/tools/nunit-lite/nunit-lite-console/nunit-lite-console.exe.config.tmpl $(TEST_NUNITLITE_APP_CONFIG_GLOBAL) $(TEST_NUNITLITE_APP_CONFIG_RUNTIME) $(TEST_NUNITLITE_APP_CONFIG_SUPPLEMENTAL) | $(test_lib_dir)
	cp -f $(topdir)/tools/nunit-lite/nunit-lite-console/nunit-lite-console.exe.config.tmpl $(test_lib_output).nunitlite.config
ifdef TEST_NUNITLITE_APP_CONFIG_GLOBAL
	sed -i -e "/__INSERT_CUSTOM_APP_CONFIG_GLOBAL__/r $(TEST_NUNITLITE_APP_CONFIG_GLOBAL)" $(test_lib_output).nunitlite.config
endif
ifdef TEST_NUNITLITE_APP_CONFIG_RUNTIME
	sed -i -e "/__INSERT_CUSTOM_APP_CONFIG_RUNTIME__/r $(TEST_NUNITLITE_APP_CONFIG_RUNTIME)" $(test_lib_output).nunitlite.config
endif
ifdef TEST_NUNITLITE_APP_CONFIG_SUPPLEMENTAL
	cp -f $(TEST_NUNITLITE_APP_CONFIG_SUPPLEMENTAL) $(test_lib_output).nunitlite.config.$(TEST_NUNITLITE_APP_CONFIG_SUPPLEMENTAL)
endif

copy-nunitlite-appconfig: $(test_lib_output).nunitlite.config
	cp -f $(test_lib_output).nunitlite.config $(NUNITLITE_CONFIG_FILE)

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
	$(MAKE) -C $(topdir)/tools/mkbundle

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
TEST_HARNESS_EXCLUDES:=$(TEST_HARNESS_EXCLUDES) StaticLinkedAotNotWorking
else 
TEST_HARNESS_EXEC=$(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(TEST_COVERAGE_FLAGS) $(AOT_RUN_FLAGS) $(TEST_HARNESS)
endif

$(test_lib_dir)/nunit-excludes.txt: $(topdir)/build/tests.make | $(test_lib_dir)
	@rm -f $@
	@$(foreach entry,$(TEST_HARNESS_EXCLUDES),echo "$(entry)" >> $@;)

## FIXME: i18n problem in the 'sed' command below
run-test-lib: test-local test-local-aot-compile copy-nunitlite-appconfig
	ok=:; \
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" MONO_REGISTRY_PATH="$(HOME)/.mono/registry" MONO_TESTS_IN_PROGRESS="yes" DBG_RUNTIME_ARGS="$(TEST_RUNTIME_FLAGS)" $(TEST_HARNESS_EXEC) $(test_assemblies) $(NOSHADOW_FLAG) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) -exclude=$(subst $(space),$(comma),$(TEST_HARNESS_EXCLUDES)) $(LABELS_ARG) -format:nunit2 -result:TestResult-$(PROFILE).xml $(FIXTURE_ARG) $(TESTNAME_ARG)|| ok=false; \
	if [ ! -f "TestResult-$(PROFILE).xml" ]; then echo "<?xml version='1.0' encoding='utf-8'?><test-results failures='1' total='1' not-run='0' name='bcl-tests' date='$$(date +%F)' time='$$(date +%T)'><test-suite name='$(strip $(test_assemblies))' success='False' time='0'><results><test-case name='$(notdir $(strip $(test_assemblies))).crash' executed='True' success='False' time='0'><failure><message>The test runner didn't produce a test result XML, probably due to a crash of the runtime. Check the log for more details.</message><stack-trace></stack-trace></failure></test-case></results></test-suite></test-results>" > TestResult-$(PROFILE).xml; fi; \
	$$ok

## Instructs compiler to compile to target .net execution, it can be usefull in rare cases when runtime detection is not possible
run-test-ondotnet-lib: LOCAL_TEST_COMPILER_ONDOTNET_FLAGS:=-d:RUN_ONDOTNET
run-test-ondotnet-lib: test-local
	ok=:; \
	$(TEST_HARNESS) $(test_assemblies) $(NOSHADOW_FLAG) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_ONDOTNET_FLAGS) /exclude:$(subst $(space),$(comma),$(TEST_HARNESS_EXCLUDES) NotDotNet) $(LABELS_ARG) -format:nunit2 -result:TestResult-ondotnet-$(PROFILE).xml $(FIXTURE_ARG) $(TESTNAME_ARG) || ok=false; \
	$$ok


endif

TEST_FILES =

ifdef HAVE_CS_TESTS
TEST_FILES += `sed -e '/^$$/d' -e 's,^../,,' -e '/^\#.*$$/d' -et -e 's,^,Test/,' $(test_sourcefile_base)`
endif

ifdef HAVE_CS_TESTS

$(test_lib_dir):
	mkdir -p $@

test_library = $(ASSEMBLY:$(ASSEMBLY_EXT)=)_test$(ASSEMBLY_EXT)

test_sourcefile = $(depsdir)/$(PROFILE_PLATFORM)_$(PROFILE)_$(test_library).sources
$(test_sourcefile): $(test_sourcefile_base) $(wildcard *_test.dll.sources) $(wildcard *_test.dll.exclude.sources) $(depsdir)/.stamp
	$(GENSOURCES) --basedir:./Test --strict --platformsdir:$(topdir)/build "$@" "$(test_library)" "$(PROFILE_PLATFORM)" "$(PROFILE)"

test_response = $(depsdir)/$(PROFILE_PLATFORM)_$(PROFILE)_$(test_library).response
$(test_response): $(test_sourcefile) $(topdir)/build/tests.make $(depsdir)/.stamp
	$(PLATFORM_CHANGE_SEPARATOR_CMD) <$(test_sourcefile) >$@

test_makefrag = $(depsdir)/$(PROFILE_PLATFORM)_$(PROFILE)_$(test_library).makefrag
$(test_makefrag): $(test_sourcefile) $(topdir)/build/tests.make $(depsdir)/.stamp
	@echo Creating $@ ...
	@sed 's,^,$(build_lib): ,' $< >$@
	@if test ! -f $(test_sourcefile).makefrag; then :; else \
	   cat $(test_sourcefile).makefrag >> $@ ; \
	   echo '$@: $(test_sourcefile).makefrag' >> $@; \
	   echo '$(test_sourcefile).makefrag:' >> $@; fi

$(test_lib_output): $(test_assembly_dep) $(test_response) $(test_nunit_dep) $(test_lib_output).nunitlite.config | $(test_lib_dir)
	$(TEST_COMPILE) $(LIBRARY_FLAGS) -target:library -out:$@ $(test_flags) $(LOCAL_TEST_COMPILER_ONDOTNET_FLAGS) @$(test_response)

tests_CLEAN_FILES += $(test_sourcefile) $(test_response) $(test_makefrag)



-include $(test_makefrag)

build-test-lib: $(test_lib_output)
	@echo Building testing lib

endif


ifdef HAVE_CS_XTESTS

XTEST_HARNESS_PATH := $(topdir)/../external/xunit-binaries
XTEST_HARNESS = $(XTEST_HARNESS_PATH)/xunit.console.exe
XTEST_RESULT_FILE := TestResult-$(PROFILE)-xunit.xml
XTEST_HARNESS_FLAGS := -noappdomain -noshadow -parallel none -nunit $(XTEST_RESULT_FILE)

XTEST_NOTRAITS := category=failing category=nonmonotests Benchmark=true

ifndef OUTER_LOOP
XTEST_NOTRAITS += category=outerloop
endif

# The logic is double inverted so this actually excludes tests not intented for current platform
# best to search for `property name="category"` in the xml output to see what's going on
# https://github.com/dotnet/buildtools/blob/master/src/xunit.netcore.extensions/Discoverers/PlatformSpecificDiscoverer.cs
XTEST_NOTRAITS += category=non$(XTEST_PLATFORM)tests

TEST_MONO_PATH := $(TEST_MONO_PATH)$(PLATFORM_PATH_SEPARATOR)$(XTEST_HARNESS_PATH)

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
xunit-test-local: $(xtest_lib_output) $(test_lib_dir)/xunit-excludes.txt $(test_lib_dir)/Xunit.NetCore.Extensions.dll $(test_lib_dir)/xunit.execution.dotnet.dll
run-xunit-test-local: run-xunit-test-lib

$(test_lib_dir)/xunit-excludes.txt: $(topdir)/build/tests.make | $(test_lib_dir)
	@rm -f $@
	$(foreach entry,$(XTEST_NOTRAITS),echo "$(entry)" >> $@;)

$(test_lib_dir)/Xunit.NetCore.Extensions.dll: $(topdir)/build/tests.make $(topdir)/../external/xunit-binaries/Xunit.NetCore.Extensions.dll | $(test_lib_dir)
	@cp -f $(topdir)/../external/xunit-binaries/Xunit.NetCore.Extensions.dll $@

$(test_lib_dir)/xunit.execution.dotnet.dll: $(topdir)/build/tests.make $(topdir)/../external/xunit-binaries/xunit.execution.dotnet.dll | $(test_lib_dir)
	@cp -f $(topdir)/../external/xunit-binaries/xunit.execution.dotnet.dll $@

run-xunit-test-lib: xunit-test-local
	ok=:; \
	PATH="$(TEST_RUNTIME_WRAPPERS_PATH):$(PATH)" REMOTE_EXECUTOR="$(XTEST_REMOTE_EXECUTOR_ABSPATH)" $(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(XTEST_COVERAGE_FLAGS) $(AOT_RUN_FLAGS) $(XTEST_HARNESS) $(xtest_lib_output) $(XTEST_HARNESS_FLAGS) -notrait $(subst $(space), -notrait ,$(XTEST_NOTRAITS)) || ok=false; \
	if [ -n "$$MONO_BABYSITTER_NUNIT_XML_LIST_FILE" ]; then echo "$(abspath $(XTEST_RESULT_FILE))" >> "$$MONO_BABYSITTER_NUNIT_XML_LIST_FILE"; fi; \
	$$ok

# Some xunit tests want to be executed in a separate process (see RemoteExecutorTestBase)
$(XTEST_REMOTE_EXECUTOR): $(topdir)/../external/corefx/src/Common/tests/System/Diagnostics/RemoteExecutorConsoleApp/RemoteExecutorConsoleApp.cs | $(test_lib_dir)
	$(TEST_COMPILE) -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll $< -out:$@

$(xtest_lib_output): $(the_assembly) $(xtest_response) $(xunit_libs_dep) $(xunit_src) $(XTEST_REMOTE_EXECUTOR) | $(test_lib_dir)
	$(TEST_COMPILE) $(LIBRARY_FLAGS) $(XTEST_LIB_FLAGS) -target:library -out:$@ $(xtest_flags) @$(xtest_response) $(xunit_src)

xtest_response_preprocessed = $(xtest_response)_preprocessed

# This handles .excludes/.sources pairs, as well as resolving the
# includes that occur in .sources files
$(xtest_response): $(xtest_sourcefile_base) $(wildcard *xtest.dll.sources) $(wildcard $(xtest_sourcefile_base_excludes))
	$(GENSOURCES) --strict --platformsdir:$(topdir)/build "$@" "$(xtest_sourcefile_base)" "$(xtest_sourcefile_base_excludes)"

$(xtest_makefrag): $(xtest_response)
	@echo Creating $@ ...
	@sed 's,^,$(xtest_lib_output): ,' $< >$@

-include $(xtest_makefrag)

endif

