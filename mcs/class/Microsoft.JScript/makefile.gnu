topdir = ../..

LIBRARY = $(TOPDIR)/class/lib/Microsoft.JScript.dll

LIB_LIST = unix.args
LIB_FLAGS = /r:antlr.runtime.dll

include $(topdir)/class/library.make