topdir = ../..

LIBRARY = ../lib/System.Xml.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System

export MONO_PATH_PREFIX = ../lib:

include ../library.make
