topdir = ../..

TEST_DIR= Test
LIBRARY = $(topdir)/class/lib/Mono.Directory.LDAP.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System -r System.Data -r mscorlib

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
