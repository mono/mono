topdir = ../../..

LIBRARY = System.XML_linux_test.dll

LIB_LIST = System.XML_linux_test.args
LIB_FLAGS = \
	-r $(topdir)/class/lib/corlib.dll	\
	-r $(topdir)/class/lib/System.Xml.dll	\
	-r $(topdir)/class/lib/NUnitCore_mono.dll

SOURCES_INCLUDE = *.cs
SOURCES_EXCLUDE = ./TheTests.cs

include $(topdir)/class/library.make

MCS_FLAGS = --target library --noconfig

TEST_SUITE_PREFIX = MonoTests.System.Xml.
TEST_SUITE = AllTests

test: $(LIBRARY)
	-MONO_PATH=$(MONO_PATH) mono $(topdir)/class/lib/NUnitConsole_mono.exe $(TEST_SUITE_PREFIX)$(TEST_SUITE),System.XML_linux_test.dll
