topdir = ../..

LIBRARY = ../lib/System.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System.Xml

export MONO_PATH_PREFIX = ../lib:

include ../library.make
