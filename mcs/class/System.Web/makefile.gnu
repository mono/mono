topdir = ../..

LIBRARY = $(topdir)/class/lib/System.Web.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System -r System.Drawing -r System.Xml

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH = $(topdir)/class/lib:

include $(topdir)/class/library.make
