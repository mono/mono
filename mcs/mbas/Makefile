thisdir = mbas
include ../build/rules.make

PROGRAM = mbas.exe

LOCAL_MCS_FLAGS = /r:System.dll /r:Mono.GetOptions.dll /r:Microsoft.VisualBasic.dll

EXTRA_DISTFILES = mb-parser.jay mbas.csproj mbas.ico mbas.sln
CLEAN_FILES = y.output 

CONSOLIDATED_TEST_RESULTS_FILE = TestResults.log

include ../build/executable.make

consolidate-test-results:
	@cp Test/tests/TestResults.log $(CONSOLIDATED_TEST_RESULTS_FILE)
	@cat Test/errors/TestResults.log >> $(CONSOLIDATED_TEST_RESULTS_FILE)

