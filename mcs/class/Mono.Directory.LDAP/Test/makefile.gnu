topdir = ../../..

LIBRARY = dslib_linux_test.dll

LIB_LIST = dslib_linux_test.args
LIB_FLAGS = -r ../../lib/Mono.Directory.LDAP.dll -r ../../lib/corlib.dll -r ../../lib/System.dll \
	    -r $(topdir)/nunit/src/NUnitCore/NUnitCore_mono.dll

include ../../library.make

MCS_FLAGS = --target library --noconfig

TEST_SUITE_PREFIX = MonoTests.Directory.LDAP.
TEST_SUITE = AllTests
NUNITCONSOLE=$(topdir)/nunit/src/NUnitConsole/NUnitConsole_mono.exe 
NUNIT_MONO_PATH=$(topdir)/nunit/src/NUnitCore:.

test: $(LIBRARY) run_test

.PHONY: run_test

run_test:
	MONO_PATH=$(NUNIT_MONO_PATH) mono $(NUNITCONSOLE) $(TEST_SUITE_PREFIX)$(TEST_SUITE),dslib_linux_test.dll
