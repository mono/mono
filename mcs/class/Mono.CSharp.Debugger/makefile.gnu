topdir = ../..

LIBRARY = ../lib/Mono.CSharp.Debugger.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib

SOURCES_INCLUDE=*.cs


export MONO_PATH_PREFIX = ../lib:

include ../library.make
