thisdir = tools/SqlSharp
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.dll /r:System.Xml.dll /r:System.Data.dll
PROGRAM = sqlsharp.exe
EXTRA_DISTFILES = README
HAS_TEST = yes
include ../../build/executable.make

test-local: $(PROGRAM)

run-test-local: $(PROGRAM)
	$(TEST_RUNTIME) $(PROGRAM) -A $(PROGRAM) $(ARGS)

run: $(PROGRAM)
	$(RUNTIME) $(PROGRAM) $(ARGS)
