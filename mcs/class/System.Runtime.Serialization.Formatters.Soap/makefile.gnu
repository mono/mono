topdir = ../..

LIBRARY = ../lib/System.Runtime.Serialization.Formatters.Soap.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System.Xml

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = ../lib:

include ../library.make
