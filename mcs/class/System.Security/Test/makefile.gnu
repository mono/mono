topdir = ../../..

LIBRARY = security_linux_test.dll

LIB_LIST = security_linux_test.args
LIB_FLAGS =	\
	-r $(topdir)/class/lib/corlib.dll \
	-r $(topdir)/class/lib/System.Xml.dll \
	-r $(topdir)/class/lib/NUnitCore_mono.dll

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=_DUMMY_

include $(topdir)/class/library.make

MCS_FLAGS = --target library --noconfig

TEST_SUITE_PREFIX = MonoTests.
TEST_SUITE = AllTests
NUNITCONSOLE=$(topdir)/class/lib/NUnitConsole_mono.exe 

test: $(LIBRARY) run_test

.PHONY: run_test

run_test:
	MONO_PATH=$(NUNIT_MONO_PATH) mono $(NUNITCONSOLE) $(TEST_SUITE_PREFIX)$(TEST_SUITE),security_linux_test.dll
