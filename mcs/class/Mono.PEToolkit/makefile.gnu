topdir = ../..

LIBRARY = $(topdir)/class/lib/Mono.PEToolkit.dll

LIB_LIST = list.unix
LIB_FLAGS = --unsafe -r corlib -r System.Xml -r mscorlib

SOURCES_INCLUDE=
SOURCES_EXCLUDE=

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
