topdir = ../..

LIBRARY = $(topdir)/class/lib/Mono.CSharp.Debugger.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib

SOURCES_INCLUDE=*.cs


export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
