topdir = ../..

LIBRARY = ../lib/System.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System.Xml -r System.Drawing

include ../library.make
