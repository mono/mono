thisdir = tools/mono-shlib-cop
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Mono.Posix.dll

PROGRAM = mono-shlib-cop.exe

TEST_INPUT = test.dll

EXTRA_DISTFILES = test.cs

include ../../build/executable.make

clean-local: cleanup

cleanup:
	-rm -f $(TEST_INPUT)

run-test-local: run-mono-shlib-cop-test

run-mono-shlib-cop-test: $(the_lib) $(TEST_INPUT)
	$(RUNTIME) $(the_lib) $(TEST_INPUT) | diff - test.dll.out

$(TEST_INPUT) : test.cs
	$(CSCOMPILE) -target:library $< -out:$@
