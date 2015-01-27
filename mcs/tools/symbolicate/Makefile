thisdir = tools/symbolicate
SUBDIRS =
include ../../build/rules.make

PROGRAM = symbolicate.exe

LOCAL_MCS_FLAGS = \
	/r:Mono.Cecil.dll	\
	/r:Mono.CompilerServices.SymbolWriter.dll \
	/r:System.Xml

include ../../build/executable.make

LIB_PATH = $(topdir)/class/lib/net_4_5

MONO = MONO_PATH=$(LIB_PATH)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH $(RUNTIME)

OUT_DIR = Test/out
TEST_CS = Test/StackTraceDumper.cs
TEST_EXE = $(OUT_DIR)/StackTraceDumper.exe
RELEASE_FILE = $(OUT_DIR)/release.out
SYMBOLICATE_FILE = $(OUT_DIR)/symbolicate.out
DEBUG_FILE = $(OUT_DIR)/debug.out

build-test:
	@mkdir -p $(OUT_DIR)
	@$(MCS) -debug $(TEST_CS) -out:$(TEST_EXE)

check: build-test
	@MONO_DEBUG=gen-compact-seq-points $(MONO) $(TEST_EXE) > $(RELEASE_FILE)
	@$(MONO) $(LIB_PATH)/symbolicate.exe $(TEST_EXE) $(RELEASE_FILE) > $(SYMBOLICATE_FILE)
	@$(MONO) --debug $(TEST_EXE) > $(DEBUG_FILE)
	@DIFF=$$(diff $(SYMBOLICATE_FILE) $(DEBUG_FILE)); \
	if [ ! -z "$$DIFF" ]; then \
		echo "Symbolicate tests failed"; \
		echo "$$DIFF"; \
		exit 1; \
	fi
