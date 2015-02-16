thisdir = tools/symbolicate
SUBDIRS =
include ../../build/rules.make

PROGRAM = symbolicate.exe

LOCAL_MCS_FLAGS = \
	/r:Mono.Cecil.dll	\
	/r:Mono.CompilerServices.SymbolWriter.dll \
	/r:System.Xml

include ../../build/executable.make

LIB_PATH = $(topdir)/class/lib/$(PROFILE)

MONO = MONO_PATH=$(LIB_PATH)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH $(RUNTIME)

OUT_DIR = Test/out
TEST_CS = Test/StackTraceDumper.cs
TEST_EXE = $(OUT_DIR)/StackTraceDumper.exe
RELEASE_FILE = $(OUT_DIR)/release.out
SYMBOLICATE_FILE = $(OUT_DIR)/symbolicate.out
SYMBOLICATE_EXPECTED_FILE = Test/symbolicate.expected

build-test:
	@mkdir -p $(OUT_DIR)
	@$(MCS) -debug $(TEST_CS) -out:$(TEST_EXE)

check: all build-test
	@MONO_DEBUG=gen-compact-seq-points $(MONO) $(TEST_EXE) > $(RELEASE_FILE)
	@$(MONO) $(LIB_PATH)/symbolicate.exe $(TEST_EXE) $(RELEASE_FILE) | sed "s/\[.*Test\//in /" > $(SYMBOLICATE_FILE)
	@DIFF=$$(diff $(SYMBOLICATE_FILE) $(SYMBOLICATE_EXPECTED_FILE)); \
	if [ ! -z "$$DIFF" ]; then \
		echo "Symbolicate tests failed."; \
		echo "If $(SYMBOLICATE_FILE) is correct copy it to $(SYMBOLICATE_EXPECTED_FILE)."; \
		echo "Otherwise runtime sequence points need to be fixed."; \
		echo "$$DIFF"; \
		exit 1; \
	fi
