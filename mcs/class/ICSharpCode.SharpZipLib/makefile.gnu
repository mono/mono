topdir = ../..

TEST_DIR= 
LIBRARY = $(topdir)/class/lib/ICSharpCode.SharpZipLib.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System -r System.Xml

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
