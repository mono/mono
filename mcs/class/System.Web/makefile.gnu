topdir = ../..

LIBRARY = ../lib/System.Web.dll

LIB_FLAGS = -r corlib -r System -r System.Drawing -r System.Xml

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*


export MONO_PATH_PREFIX = ../lib:

include ../library.make
