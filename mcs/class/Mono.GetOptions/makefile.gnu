topdir = ../..

LIBRARY = $(topdir)/class/lib/Mono.GetOptions.dll

LIB_LIST = list.unix
LIB_FLAGS = 

SOURCES_INCLUDE=*.cs


export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
