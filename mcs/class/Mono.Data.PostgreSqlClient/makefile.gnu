topdir = ../..

LIBRARY = $(topdir)/class/lib/Mono.Data.PostgreSqlClient.dll

LIB_LIST = list
LIB_FLAGS = -r corlib.dll -r System.dll -r System.Xml.dll -r System.Data.dll

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
