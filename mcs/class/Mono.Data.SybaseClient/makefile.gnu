topdir = ../..

LIBRARY = $(topdir)/class/lib/Mono.Data.SybaseClient.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System -r System.Xml -r System.EnterpriseServices -r System.Data

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\
	*TestGDA.cs	\
	./System.Xml*

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
