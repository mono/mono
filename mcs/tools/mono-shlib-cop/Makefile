thisdir = tools/mono-shlib-cop
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Mono.Posix.dll -r:Mono.GetOptions.dll

PROGRAM = mono-shlib-cop.exe

TEST_INPUT = test.dll

include ../../build/executable.make

test-local: $(PROGRAM) $(TEST_INPUT)

clean-local: cleanup

cleanup:
	-rm $(TEST_INPUT)

run-test-local: run-mono-shlib-cop-test

run-mono-shlib-cop-test: $(PROGRAM) $(TEST_INPUT)
	$(RUNTIME) $(PROGRAM) $(TEST_INPUT) | diff - test.dll.out

$(TEST_INPUT) : test.cs
	$(CSCOMPILE) -target:library $< -out:$@

