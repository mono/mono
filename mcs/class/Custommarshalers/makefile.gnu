topdir = ../..

LIBRARY = $(topdir)/class/lib/Custommarshalers.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH = $(topdir)/class/lib:

include $(topdir)/class/library.make
