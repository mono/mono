topdir = ../..

LIBRARY = ../lib/System.Xml.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\
	./System.Xml/Driver.cs

export MONO_PATH_PREFIX = ../lib:

include ../library.make
