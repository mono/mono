topdir = ../..

TEST_DIR = Test
LIBRARY = $(topdir)/class/lib/System.Windows.Forms.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System -r System.Drawing -r Accessibility /nowarn:0114 /nowarn:0108

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\
	./System.Xml/Driver.cs

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
