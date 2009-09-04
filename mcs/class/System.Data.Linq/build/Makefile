thisdir = class/System.Data.Linq/build
include ../../../build/rules.make

ASSEMBLY     = System.Data.Linq.dll
ASSEMBLY_EXT = .dll
the_assembly = $(topdir)/class/lib/$(PROFILE)/$(ASSEMBLY)

include ../../../build/tests.make

TEST_SOURCES_WITH_SPACES = \
	../src/DbLinq/Test/Providers/Linq_101_Samples/Object\ Loading.cs

sqlite_tests = DbLinq.Sqlite_test_mono_strict.dll

dblinq_deps = \
	$(test_nunit_dep)                    \
	$(TEST_SOURCES_WITH_SPACES)          \
	$(the_assembly)                      \
	Makefile

sqlite_tests_dep = \
	$(sqlite_tests).sources              \
	$(shell cat $(sqlite_tests).sources) \
	$(dblinq_deps)

$(sqlite_tests): $(sqlite_tests_dep)
	$(TEST_COMPILE) -target:library -out:$@ \
		-r:System.Data.dll -r:$(the_assembly) -r:Mono.Data.Sqlite $(test_nunit_ref) \
		$(TEST_MCS_FLAGS) -d:MONO_STRICT -d:SQLITE \
		@$< $(TEST_SOURCES_WITH_SPACES)

test-sqlite: $(sqlite_tests)

RUN_TEST_COMMAND = \
	MONO_REGISTRY_PATH="$(HOME)/.mono/registry" $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(TEST_HARNESS) $(1) -noshadow $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) $(TEST_HARNESS_EXCLUDES) $(TEST_HARNESS_OUTPUT) -xml=TestResult-$(1:.dll=)-$(PROFILE).xml -out:TestResult-$(1:.dll=)-$(PROFILE).out $(FIXTURE_ARG) $(TESTNAME_ARG) ; \
	xsltproc $(topdir)/build/nunit-summary.xsl TestResult-$(1)-$(PROFILE).xml > TestResult-$(1:.dll=)-$(PROFILE).log

run-test-sqlite:
	$(call RUN_TEST_COMMAND,$(sqlite_tests))

foo:
	MONO_REGISTRY_PATH="$(HOME)/.mono/registry" $(TEST_RUNTIME) $(RUNTIME_FLAGS) $(TEST_HARNESS) $(sqlite_tests) -noshadow $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) $(TEST_HARNESS_EXCLUDES) $(TEST_HARNESS_OUTPUT) -xml=TestResult-$(PROFILE).xml $(FIXTURE_ARG) $(TESTNAME_ARG)|| ok=false; \
	$(TEST_HARNESS_POSTPROC) ; $$ok

all-local: $(sqlite_tests)

test-local: test-sqlite

run-test-local: test-local run-test-sqlite

clean-local:
	-rm -f Northwind.db3 $(sqlite_tests) $(sqlite_tests).mdb TestResult-*

