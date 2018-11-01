thisdir = tools/linker
SUBDIRS =
include ../../build/rules.make

PROGRAM = monolinker.exe

LIB_REFS = System System.Core System.Xml Mono.Cecil

TEST_CASES := \
	mscorlib/test-array.cs \
	mscorlib/test-calendar-01.cs \
	mscorlib/test-calendar-02.cs \
	mscorlib/test-exception-01.cs \
	mscorlib/test-locale-01.cs \
	mscorlib/test-methodimpl-01.cs \
	mscorlib/test-reflection-01.cs \
	mscorlib/test-reflection-02.cs \
	mscorlib/test-string-01.cs \
	mscorlib/test-string-02.cs \
	mscorlib/test-string-03.cs \
	mscorlib/test-task-01.cs \
	System/test-security.cs

# Disabled tests
#	mscorlib/test-marshaling.cs

ifdef MOBILE_PROFILE
TEST_CASES += \
	mscorlib/test-crypto-01.cs \
	System.Net.Http/test-handler-01.cs
endif

ifndef AOT_FRIENDLY_PROFILE
TEST_CASES += \
	mscorlib/test-remoting.cs \
	mscorlib/test-reflection.cs
endif

SIZE_TEST_CASES :=

TESTS_COMPILER = $(MCS) -nologo -noconfig -unsafe -debug:portable -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll

MARSHALING_TEST_NATIVE = mscorlib/test-marshaling-native.so

check: compile-tests
	$(MAKE) run-tests

compile-tests: $(TEST_CASES) $(SIZE_TEST_CASES)

mscorlib/test-%.cs:
	$(TESTS_COMPILER) Tests/$@ /out:Tests/$(@:.cs=.exe)

$(MARSHALING_TEST_NATIVE): $(@:.so=.c)
	$(CCOMPILE) Tests/$(@:.so=.c) -shared -o Tests/$@

mscorlib/test-marshaling.cs: $(MARSHALING_TEST_NATIVE)
	$(TESTS_COMPILER) Tests/$@ /out:Tests/$(@:.cs=.exe)

System/test-%.cs:
	$(TESTS_COMPILER)  -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.dll Tests/$@ /out:Tests/$(@:.cs=.exe)

System.Net.Http/test-%.cs:
	$(TESTS_COMPILER)  -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.dll -r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Net.Http.dll Tests/$@ /out:Tests/$(@:.cs=.exe)

run-tests: $(TEST_CASES:.cs=.exe)

LINKER_PROFILE_OPTIONS :=

# Should be in sync with runtime sdk flags
ifeq ($(PROFILE),monotouch)
LINKER_PROFILE_OPTIONS += --exclude-feature remoting --exclude-feature com
endif

ifeq ($(PROFILE),wasm)
LINKER_PROFILE_OPTIONS += --exclude-feature remoting --exclude-feature com --exclude-feature sre
endif

LINKER_OUTPUT := illink-output-$(PROFILE_DIRECTORY)
PROFILE_PATH = $(topdir)/class/lib/$(PROFILE_DIRECTORY)
LINKER = MONO_PATH=$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE) $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/monolinker.exe -c link -out $(LINKER_OUTPUT) -b true -d $(PROFILE_PATH)
LINKER_DEFAULT = $(LINKER) -l none $(LINKER_PROFILE_OPTIONS)
TEST_EXEC = MONO_PATH=$(LINKER_OUTPUT) $(RUNTIME) $(RUNTIME_FLAGS) --debug -O=-aot

.NOTPARALLEL:

System.Net.Http/test-%.exe System/test-%.exe mscorlib/test-%.exe:
	@rm -rf $(LINKER_OUTPUT)
	@mkdir $(LINKER_OUTPUT)
	@echo Testing $@
	$(LINKER_DEFAULT) -a Tests/$@
	$(TEST_EXEC) $(LINKER_OUTPUT)/$(@F)
	@rm -rf $(LINKER_OUTPUT)

mscorlib/test-calendar-01.exe:
	@rm -rf $(LINKER_OUTPUT)
	@mkdir $(LINKER_OUTPUT)
	@echo Testing $@
	$(LINKER) -l mideast $(LINKER_PROFILE_OPTIONS) -a Tests/$@
	$(TEST_EXEC) $(LINKER_OUTPUT)/$(@F)
	@rm -rf $(LINKER_OUTPUT)

mscorlib/test-marshaling.exe: $(MARSHALING_TEST_NATIVE)
	@rm -rf $(LINKER_OUTPUT)
	@mkdir $(LINKER_OUTPUT)
	@echo Testing $@
	$(LINKER_DEFAULT) -a Tests/$@
	cp Tests/$(MARSHALING_TEST_NATIVE) $(LINKER_OUTPUT)
	(cd $(LINKER_OUTPUT); MONO_PATH=. $(RUNTIME) $(RUNTIME_FLAGS) --debug -O=-aot ./$(@F))
	@rm -rf $(LINKER_OUTPUT)

BCL_ASSEMBLIES_CORE=mscorlib.dll System.dll System.Core.dll System.Xml.dll
BCL_ASSEMBLIES=$(BCL_ASSEMBLIES_CORE) $(sort $(filter-out $(BCL_ASSEMBLIES_CORE), $(notdir $(wildcard $(PROFILE_PATH)/System.*.dll)) ))

COMMA=,
REPORT_FILE=$(PROFILE)-linked-size.csv

bcl-size-current: compile-tests
	@echo "App,$$(echo '$(BCL_ASSEMBLIES)' | tr ' ' ',')" > $(REPORT_FILE)
	@for app in $(sort $(SIZE_TEST_CASES:.cs=.exe) $(TEST_CASES:.cs=.exe)); do \
		rm -rf $(LINKER_OUTPUT); \
		mkdir $(LINKER_OUTPUT); \
		echo Checking linked BCL size for $$app; \
		$(LINKER_DEFAULT) -a Tests/$$app; \
		sizes=""; \
		for asm in $(BCL_ASSEMBLIES); do \
			if [ -f "$(LINKER_OUTPUT)/$$asm" ]; then size=$$(wc -c < "$(LINKER_OUTPUT)/$$asm" | tr -d "[:space:]"); else size="0"; fi; \
			sizes="$$sizes,$$size"; \
		done; \
		echo "$$app$$sizes" >> $(REPORT_FILE); \
		rm -rf $(LINKER_OUTPUT); \
	done;

bcl-size-diff:
	@echo "Regenerating BCL Linked Size diff..."
	$(Q) $(MAKE) bcl-size-current PROFILE=net_4_x   || true
	$(Q) $(MAKE) bcl-size-current PROFILE=monotouch || true
	$(Q) $(MAKE) bcl-size-current PROFILE=monodroid || true
	$(Q) $(MAKE) bcl-size-current PROFILE=wasm || true
	@echo "Checking size differences..."
	@mkdir -p sizediff
	$(Q) git diff HEAD "*.csv" > temp.patch
	$(Q) git show HEAD:./net_4_x-linked-size.csv > net_4_x-linked-size.old.csv
	$(Q) git show HEAD:./monotouch-linked-size.csv > monotouch-linked-size.old.csv
	$(Q) git show HEAD:./monodroid-linked-size.csv > monodroid-linked-size.old.csv
	$(Q) git show HEAD:./wasm-linked-size.csv > wasm-linked-size.old.csv
	$(Q) sed -e "/@diffdata@/r temp.patch" -e "/@diffdata@/d" -e "/@olddata-net_4_x@/r net_4_x-linked-size.old.csv" -e "/@olddata-net_4_x@/d" -e "/@olddata-monotouch@/r monotouch-linked-size.old.csv" -e "/@olddata-monotouch@/d" -e "/@olddata-monodroid@/r monodroid-linked-size.old.csv" -e "/@olddata-monodroid@/d" -e "/@olddata-wasm@/r wasm-linked-size.old.csv" -e "/@olddata-wasm@/d" -e "/@newdata-net_4_x@/r net_4_x-linked-size.csv" -e "/@newdata-net_4_x@/d" -e "/@newdata-monotouch@/r monotouch-linked-size.csv" -e "/@newdata-monotouch@/d" -e "/@newdata-monodroid@/r monodroid-linked-size.csv" -e "/@newdata-monodroid@/d" -e "/@newdata-wasm@/r wasm-linked-size.csv" -e "/@newdata-wasm@/d" linked-size-diff.html.in > sizediff/index.html
	$(Q) if [ -s temp.patch ]; then echo "Error: Found BCL Linked Size differences, see mcs/tools/linker/sizediff/index.html."; rm -f temp.patch *.old.csv; exit 1; else echo "No differences found."; rm -f temp.patch *.old.csv; fi

include ../../build/executable.make
