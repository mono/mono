thisdir = tools/SqlSharp
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.dll /r:System.Xml.dll /r:System.Data.dll
PROGRAM = sqlsharp.exe
EXTRA_DISTFILES = README

include ../../build/executable.make

# Disable test for now
#run-test-local: run-sqlsharp-test

run-sqlsharp-test: $(PROGRAM)
	$(TEST_RUNTIME) $(PROGRAM) -A $(PROGRAM) $(ARGS)

run: $(PROGRAM)
	$(RUNTIME) $(PROGRAM) $(ARGS)
