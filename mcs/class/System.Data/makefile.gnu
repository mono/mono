topdir = ../..

LIBRARY = ../lib/System.Data.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System -r System.Xml

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\
	*TestGDA.cs	\
	./System.Xml*

export MONO_PATH_PREFIX = ../lib:

include ../library.make
