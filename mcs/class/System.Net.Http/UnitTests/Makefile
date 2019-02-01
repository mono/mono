thisdir = class/System.Net.Http/UnitTests
SUBDIRS = 
include ../../../build/rules.make

ASSEMBLY = System.Net.Http.UnitTests

XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks
USE_XTEST_REMOTE_EXECUTOR = YES
XTEST_MCS_FLAGS = -nowarn:436

include ../../../build/tests.make

xtest_flags = $(xunit_libs_ref) $(XTEST_MCS_FLAGS) $(XTEST_LIB_MCS_FLAGS) /unsafe

clean-local:
	-rm -f $(tests_CLEAN_FILES) $(CLEAN_FILES)

