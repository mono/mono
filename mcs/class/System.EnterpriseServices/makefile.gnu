topdir = ../..

LIBRARY = ../lib/System.EnterpriseServices.dll

LIB_FLAGS = -r corlib 

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*

export MONO_PATH_PREFIX = ../lib:

include ../library.make
