thisdir = mbas
SUBDIRS = Test
include ../build/rules.make

PROGRAM = mbas.exe
BUILT_SOURCES = mb-parser.cs

LOCAL_MCS_FLAGS = /r:System.dll /r:$(topdir)/class/lib/$(PROFILE)/Mono.GetOptions.dll /r:$(topdir)/class/lib/$(PROFILE)/Microsoft.VisualBasic.dll

EXTRA_DISTFILES = mb-parser.jay mbas.csproj mbas.ico mbas.sln
CLEAN_FILES = y.output 

CONSOLIDATED_TEST_RESULTS_FILE = TestResults.log

include ../build/executable.make

mb-parser.cs: mb-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv <$(topdir)/jay/skeleton.cs $< >$@

consolidate-test-results:
	@cp Test/tests/TestResults.log $(CONSOLIDATED_TEST_RESULTS_FILE)
	@cat Test/errors/TestResults.log >> $(CONSOLIDATED_TEST_RESULTS_FILE)

