topdir = ../..

TEST_DIR = Test
LIBRARY = $(topdir)/class/lib/System.Runtime.Serialization.Formatters.Soap.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System -r System.Xml

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
