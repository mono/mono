topdir = ../..

#TEST_DIR = Test
LIBRARY = $(topdir)/class/lib/Mono.Data.SqliteClient.dll

LIB_LIST = sources.list
LIB_FLAGS = --unsafe -r System.Data

SOURCES_INCLUDE=*.cs

export MONO_PATH_PREFIX = $(topdir)/class/lib

include $(topdir)/class/library.make
