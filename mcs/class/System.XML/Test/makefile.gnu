topdir = ../../..

LIBRARY = System.XML_linux_test.dll

LIB_LIST = System.XML_linux_test.args
LIB_FLAGS = -r ../../lib/corlib.dll -r ../../lib/System.Xml.dll -r ../../lib/NUnitCore_mono.dll

include ../../library.make

MCS_FLAGS = --target library --noconfig

test: $(LIBRARY)
	mono /usr/local/bin/NUnitConsole_mono.exe MonoTests.System.Xml.AllTests,System.XML_linux_test.dll
