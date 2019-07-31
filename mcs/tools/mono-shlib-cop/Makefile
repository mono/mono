thisdir = tools/mono-shlib-cop
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS =
LIB_REFS = Mono.Posix System System.Xml

PROGRAM = mono-shlib-cop.exe

TEST_INPUT = test.dll

EXTRA_DISTFILES = test.cs

include ../../build/executable.make

clean-local: cleanup

cleanup:
	-rm -f $(TEST_INPUT)

run-test-local: run-mono-shlib-cop-test

run-mono-shlib-cop-test: $(the_lib) $(TEST_INPUT)
ifeq ($(HOST_PLATFORM),linux)
	MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(the_lib) $(TEST_INPUT) | diff - test.dll.out
else
	@echo "mono-shlib-cop currently only works on Linux, skipping tests."
endif

$(TEST_INPUT) : test.cs
	$(CSCOMPILE) -target:library -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll $< -out:$@
