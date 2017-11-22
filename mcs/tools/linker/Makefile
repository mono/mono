thisdir = tools/linker
SUBDIRS =
include ../../build/rules.make

PROGRAM = monolinker.exe

LIB_REFS = System System.Core System.Xml Mono.Cecil

TEST_CASES = \
	mscorlib/test-array.cs \
	mscorlib/test-remoting.cs

ifndef AOT_FRIENDLY_PROFILE
TEST_CASES += \
	mscorlib/test-reflection.cs
endif

TESTS_COMPILER = $(MCS) -nologo -noconfig -debug:portable -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll

check: compile-tests
	$(MAKE) run-tests

compile-tests: $(TEST_CASES)

mscorlib/test-%.cs:
	$(TESTS_COMPILER) Tests/$@ /out:Tests/$(@:.cs=.exe)

run-tests: $(TEST_CASES:.cs=.exe)

LINKER_OUTPUT := illink-output-$(PROFILE_DIRECTORY)
PROFILE_PATH = $(topdir)/class/lib/$(PROFILE_DIRECTORY)
LINKER = MONO_PATH=$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE) $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/monolinker.exe -c link -out $(LINKER_OUTPUT) -b true -d $(PROFILE_PATH)
TEST_EXEC = MONO_PATH=$(LINKER_OUTPUT) $(RUNTIME) $(RUNTIME_FLAGS) --debug -O=-aot

.NOTPARALLEL:

mscorlib/test-%.exe:
	@rm -rf $(LINKER_OUTPUT)
	@mkdir $(LINKER_OUTPUT)
	@echo Testing $@
	$(LINKER) -a Tests/$@
	$(TEST_EXEC) $(LINKER_OUTPUT)/$(@F)
	@rm -rf $(LINKER_OUTPUT)

include ../../build/executable.make
