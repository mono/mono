thisdir = mbas
SUBDIRS = Test/misc Test/tests Test/errors
include ../build/rules.make

PROGRAM = mbas.exe
BUILT_SOURCES = mb-parser.cs

LOCAL_MCS_FLAGS = /r:System.dll /r:$(topdir)/class/lib/$(PROFILE)/Mono.GetOptions.dll

EXTRA_DISTFILES = mb-parser.jay mbas.csproj mbas.ico mbas.sln
CLEAN_FILES = y.output 

PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/1.0

CONSOLIDATED_TEST_RESULTS_FILE = TestResults.log

include ../build/executable.make

mb-parser.cs: mb-parser.jay
	$(topdir)/jay/jay -ctv <$(topdir)/jay/skeleton.cs $< >$@

consolidate-test-results:
	@cp Test/tests/TestResults.log $(CONSOLIDATED_TEST_RESULTS_FILE)
	@cat Test/errors/TestResults.log >> $(CONSOLIDATED_TEST_RESULTS_FILE)

