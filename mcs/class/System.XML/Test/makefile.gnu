topdir = ../../..

LIBRARY = System.XML_linux_test.dll

LIB_LIST = System.XML_linux_test.args
LIB_FLAGS = -r ../../lib/corlib.dll -r ../../lib/System.Xml.dll -r ../../lib/NUnitCore_mono.dll

include ../../library.make

MCS_FLAGS = --target library --noconfig

TEST_SUITE_PREFIX = MonoTests.System.Xml.
TEST_SUITE = AllTests

test: $(LIBRARY)
	mono ../../../nunit/src/NUnitConsole/NUnitConsole_mono.exe $(TEST_SUITE_PREFIX)$(TEST_SUITE),System.XML_linux_test.dll
