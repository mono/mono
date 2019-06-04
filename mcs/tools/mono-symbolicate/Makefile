thisdir = tools/mono-symbolicate
SUBDIRS =
include ../../build/rules.make

PROGRAM = mono-symbolicate.exe

LOCAL_MCS_FLAGS = /D:NO_AUTHENTICODE /D:CECIL -nowarn:649 -unsafe

LIB_REFS = System.Xml System.Core System

include ../../build/executable.make

LIB_PATH = $(topdir)/class/lib/$(PROFILE)

ifdef TARGET_NET_REFERENCE
LIB_REF_PATH = $(topdir)/../external/binary-reference-assemblies/$(TARGET_NET_REFERENCE)
else
LIB_REF_PATH = $(LIB_PATH)
endif

MONO = MONO_PATH="$(LIB_PATH)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) -O=-inline

MSYM_DIR = $(OUT_DIR)/msymdir
TEST_CS = Test/StackTraceDumper.cs
TEST_EXE = StackTraceDumper.exe
STACKTRACE_FILE = $(OUT_DIR)/stacktrace.out
SYMBOLICATE_RAW_FILE = $(OUT_DIR)/symbolicate_raw.out
SYMBOLICATE_RESULT_FILE = $(OUT_DIR)/symbolicate.result
SYMBOLICATE_EXPECTED_FILE = Test/symbolicate.expected

CHECK_DIFF = @\
	$(MONO) $(TEST_EXE) > $(STACKTRACE_FILE); \
	$(MONO) $(LIB_PATH)/$(PROGRAM) $(MSYM_DIR) $(STACKTRACE_FILE) > $(SYMBOLICATE_RAW_FILE); \
	cat $(SYMBOLICATE_RAW_FILE) | tr '\\' '/' | sed "s/) .* in .*\/mcs\//) in mcs\//" | sed "s/) .* in .*\/external\//) in external\//" | sed '/\[MVID\]/d' | sed '/\[AOTID\]/d' > $(SYMBOLICATE_RESULT_FILE); \
	DIFF=$$(diff -up $(SYMBOLICATE_EXPECTED_FILE) $(SYMBOLICATE_RESULT_FILE)); \
	if [ ! -z "$$DIFF" ]; then \
		echo "Symbolicate tests failed."; \
		echo "If $(SYMBOLICATE_RESULT_FILE) is correct copy it to $(SYMBOLICATE_EXPECTED_FILE)."; \
		echo "Otherwise runtime sequence points need to be fixed."; \
		echo "$$DIFF"; \
		exit 1; \
	fi

ifdef MCS_MODE
CHECK_DIFF = @echo "WARNING: running in mcs mode, tests are specific to roslyn and would fail. Skipping diff check."
endif

PREPARE_OUTDIR = @\
	rm -rf $(OUT_DIR); \
	mkdir -p $(OUT_DIR); \
	mkdir -p $(MSYM_DIR);

STORE_SYMBOLS = \
	$(MONO) $(LIB_PATH)/$(PROGRAM) store-symbols $(MSYM_DIR) $(OUT_DIR); \
	$(MONO) $(LIB_PATH)/$(PROGRAM) store-symbols $(MSYM_DIR) $(LIB_PATH);

COPY_TEST = @\
	cp $(TEST_EXE) $(OUT_DIR); \
	cp $(TEST_EXE:.exe=.pdb) $(OUT_DIR);

check: run-test

AOT_SUPPORTED = $(shell $(MONO) --aot 2>&1 | grep -q "AOT compilation is not supported" && echo 0 || echo 1)

test-local: $(TEST_EXE)

test-bundle:
	mkdir -p $(TEST_BUNDLE_PATH)/tests/symbolicate
	cp -L $(TEST_EXE) $(TEST_EXE:.exe=.pdb) $(SYMBOLICATE_EXPECTED_FILE) $(TEST_BUNDLE_PATH)/tests/symbolicate/

$(TEST_EXE): $(TEST_CS)
	$(CSCOMPILE) $(TEST_CS) -r:$(LIB_REF_PATH)/mscorlib.dll -r:$(LIB_REF_PATH)/System.Core.dll -warn:0 -out:$(TEST_EXE)

run-test-local: run-test-without-aot run-test-with-aot run-test-with-aot-msym

run-test-without-aot: OUT_DIR = Test/without_aot
run-test-without-aot: all
	@echo "Checking $(TEST_EXE) without AOT in $(OUT_DIR)"
	$(PREPARE_OUTDIR)
	$(COPY_TEST)
	$(STORE_SYMBOLS)
	$(CHECK_DIFF)

run-test-with-aot: OUT_DIR = Test/with_aot
run-test-with-aot: all
ifeq ($(AOT_SUPPORTED), 1)
	@echo "Checking $(TEST_EXE) with AOT in $(OUT_DIR)"
	$(PREPARE_OUTDIR)
	$(COPY_TEST)
	$(STORE_SYMBOLS)
	@$(MONO) --aot $(OUT_DIR)/$(TEST_EXE) > /dev/null
	$(CHECK_DIFF)
endif

run-test-with-aot-msym: OUT_DIR = Test/with_aot_msym
run-test-with-aot-msym: all
ifeq ($(AOT_SUPPORTED), 1)
	@echo "Checking $(TEST_EXE) with AOT (using .msym) in $(OUT_DIR)"
	$(PREPARE_OUTDIR)
	$(COPY_TEST)
	$(STORE_SYMBOLS)
	@$(MONO) --aot=msym-dir=$(MSYM_DIR) $(OUT_DIR)/$(TEST_EXE) > /dev/null
	$(CHECK_DIFF)
endif
