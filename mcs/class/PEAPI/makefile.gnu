topdir = ../..

TEST_DIR= Test
LIBRARY = $(topdir)/class/lib/PEAPI.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System

SOURCES_INCLUDE=PEAPI.cs
SOURCES_EXCLUDE=

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
