topdir = ../..

LIBRARY = ../lib/System.Configuration.Install.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib 

export MONO_PATH_PREFIX = ../lib:

include ../library.make
