topdir = ../..

LIBRARY = $(topdir)/class/lib/Microsoft.Vsa.dll

LIB_LIST = unix.args
LIB_FLAGS = /r:System.dll

SOURCES_INCLUDE = *.cs

include $(topdir)/class/library.make
