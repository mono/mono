topdir = ../..

LIBRARY = ../lib/System.Runtime.Remoting.dll

LIB_LIST = unix.args
LIB_FLAGS = -r corlib -r System

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = ../lib:

include ../library.make
