topdir = ../..

LIBRARY = $(topdir)/class/lib/System.Configuration.Install.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System.dll

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
