thisdir = tools/mono-symbolicate
SUBDIRS =
include ../../build/rules.make

PROGRAM = mono-symbolicate.exe

LOCAL_MCS_FLAGS = \
	/D:NO_AUTHENTICODE /D:CECIL

LIB_REFS = System.Xml System.Core System

include ../../build/executable.make

LIB_PATH = $(topdir)/class/lib/$(PROFILE)

MONO = MONO_PATH="$(LIB_PATH)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) -O=-inline

MSYM_DIR = $(OUT_DIR)/msymdir
TEST_CS = Test/StackTraceDumper.cs
TEST_EXE = $(OUT_DIR)/StackTraceDumper.exe
STACKTRACE_FILE = $(OUT_DIR)/stacktrace.out
SYMBOLICATE_RAW_FILE = $(OUT_DIR)/symbolicate_raw.out
SYMBOLICATE_RESULT_FILE = $(OUT_DIR)/symbolicate.result
SYMBOLICATE_EXPECTED_FILE = Test/symbolicate.expected

CHECK_DIFF = @\
	$(MONO) $(TEST_EXE) > $(STACKTRACE_FILE); \
	$(MONO) $(LIB_PATH)/$(PROGRAM) $(MSYM_DIR) $(STACKTRACE_FILE) > $(SYMBOLICATE_RAW_FILE); \
	sed "s/) .* in .*\/mcs\//) in mcs\//" $(SYMBOLICATE_RAW_FILE) | sed '/\[MVID\]/d' | sed '/\[AOTID\]/d' > $(SYMBOLICATE_RESULT_FILE); \
	DIFF=$$(diff -up $(SYMBOLICATE_EXPECTED_FILE) $(SYMBOLICATE_RESULT_FILE)); \
	if [ ! -z "$$DIFF" ]; then \
		echo "Symbolicate tests failed."; \
		echo "If $(SYMBOLICATE_RESULT_FILE) is correct copy it to $(SYMBOLICATE_EXPECTED_FILE)."; \
		echo "Otherwise runtime sequence points need to be fixed."; \
		echo "$$DIFF"; \
		exit 1; \
	fi

PREPARE_OUTDIR = @\
	rm -rf $(OUT_DIR); \
	mkdir -p $(OUT_DIR); \
	mkdir -p $(MSYM_DIR);

COMPILE = \
	$(CSCOMPILE) $(TEST_CS) -out:$(TEST_EXE); \
	$(MONO) $(LIB_PATH)/$(PROGRAM) store-symbols $(MSYM_DIR) $(OUT_DIR); \
	$(MONO) $(LIB_PATH)/$(PROGRAM) store-symbols $(MSYM_DIR) $(LIB_PATH);

check: test-local

AOT_SUPPORTED = $(shell $(MONO) --aot 2>&1 | grep -q "AOT compilation is not supported" && echo 0 || echo 1)

test-local: test-without-aot test-with-aot test-with-aot-msym

test-without-aot: OUT_DIR = Test/without_aot
test-without-aot: all
	@echo "Checking $(TEST_EXE) without AOT in $(OUT_DIR)"
	$(PREPARE_OUTDIR)
	$(COMPILE)
	$(CHECK_DIFF)

test-with-aot: OUT_DIR = Test/with_aot
test-with-aot: all
ifeq ($(AOT_SUPPORTED), 1)
	@echo "Checking $(TEST_EXE) with AOT in $(OUT_DIR)"
	$(PREPARE_OUTDIR)
	$(COMPILE)
	@$(MONO) --aot $(TEST_EXE) > /dev/null
	$(CHECK_DIFF)
endif

test-with-aot-msym: OUT_DIR = Test/with_aot_msym
test-with-aot-msym: all
ifeq ($(AOT_SUPPORTED), 1)
	@echo "Checking $(TEST_EXE) with AOT (using .msym) in $(OUT_DIR)"
	$(PREPARE_OUTDIR)
	$(COMPILE)
	@$(MONO) --aot=msym-dir=$(MSYM_DIR) $(TEST_EXE) > /dev/null
	$(CHECK_DIFF)
endif
