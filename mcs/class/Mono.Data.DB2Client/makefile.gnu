topdir = ../..

LIBRARY = $(topdir)/class/lib/Mono.Data.DB2Client.dll

LIB_LIST = list
LIB_FLAGS = --unsafe -r corlib -r System -r System.Xml -r System.Data

SOURCES_INCLUDE=*.cs

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
