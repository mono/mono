topdir = ../..

LIBRARY = ../lib/System.EnterpriseServices.dll

LIB_LIST = list
LIB_FLAGS = -r corlib 

export MONO_PATH_PREFIX = ../lib:

include ../library.make
