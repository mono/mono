topdir = ../..

LIBRARY = ../lib/Mono.Directory.LDAP.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System -r System.Data -r mscorlib

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*

export MONO_PATH_PREFIX = ../lib:

include ../library.make
