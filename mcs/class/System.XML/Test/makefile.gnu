topdir = ../../..

LIBRARY = System.XML_linux_test.dll

LIB_LIST = System.XML_linux_test.args
LIB_FLAGS = -r /usr/local/lib/corlib.dll -r ../../lib/System.Xml.dll -r ../../lib/NUnitCore_mono.dll

include ../../library.make

MCS_FLAGS = --target library --noconfig

