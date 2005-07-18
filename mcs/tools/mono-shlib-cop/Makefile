thisdir = tools/mono-shlib-cop
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Mono.Posix.dll -r:Mono.GetOptions.dll

PROGRAM = mono-shlib-cop.exe

include ../../build/executable.make

test-local: $(PROGRAM)

run-test-local: run-mono-shlib-cop-test

run-mono-shlib-cop-test: $(PROGRAM)
	$(RUNTIME) $< $< | diff - mono-shlib-cop.exe.out

