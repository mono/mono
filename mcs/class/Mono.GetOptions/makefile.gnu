topdir = ../..

LIBRARY = ../lib/Mono.GetOptions.dll

LIB_LIST = list.unix
LIB_FLAGS = -r System.Data -r System.Xml

SOURCES_INCLUDE=*.cs


export MONO_PATH_PREFIX = ../lib:

include ../library.make
