topdir = ../..

#TEST_DIR = Test
LIBRARY = $(topdir)/class/lib/Mono.Data.SqliteClient.dll

LIB_LIST = sources.list
LIB_FLAGS = --unsafe -r System.Data

SOURCES_INCLUDE=*.cs

include $(topdir)/class/library.make
