topdir = ../..

LIBRARY = ../lib/System.Configuration.Install.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib 

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = ../lib:

include ../library.make
