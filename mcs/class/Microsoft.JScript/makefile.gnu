topdir = ../..

LIBRARY = $(topdir)/class/lib/Microsoft.JScript.dll

LIB_LIST = unix.args
LIB_FLAGS = /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:Microsoft.Vsa.dll /r:System.dll

SOURCES_INCLUDE = *.cs

include $(topdir)/class/library.make
