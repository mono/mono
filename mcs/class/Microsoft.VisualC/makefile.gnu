topdir = ../..

LIBRARY = $(topdir)/class/lib/Microsoft.VisualC.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
