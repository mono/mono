topdir = ../..

LIBRARY = ../lib/System.Web.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System -r System.Drawing -r System.Xml

export MONO_PATH_PREFIX = ../lib:

include ../library.make
