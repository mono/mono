thisdir = tools/sqlsharp
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS =
LIB_REFS = System System.Xml System.Data
PROGRAM = sqlsharp.exe

EXTRA_DISTFILES = README

include ../../build/executable.make

# Disable test for now
#run-test-local: run-sqlsharp-test

run-sqlsharp-test: $(PROGRAM)
	$(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(PROGRAM) -A $(PROGRAM) $(ARGS)

run: $(PROGRAM)
	$(RUNTIME) $(RUNTIME_FLAGS) $(PROGRAM) $(ARGS)
