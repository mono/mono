#
# Makefile for System.Data.OracleClient.gluelib.so
# using gcc
#

CC=/usr/bin/gcc
TARGET=libociglue.so

ORACLE_CFLAGS = -I$(ORACLE_HOME)/rdbms/demo -I$(ORACLE_HOME)/rdbms/public -I$(ORACLE_HOME)/plsql/public -I$(ORACLE_HOME)/network/public
ORACLE_LIBS = -L$(ORACLE_HOME)/lib -lm -ldl -lserver8 -lclient8 -lgeneric8 -lcommon8 -lvsn8 -lagent8 -lmm -lclntst8 -lslax8 -lsql8 -lcore8 -lnls8 -lplc8 -lplp8 -lpls8 -lpsa8

GLIB_CFLAGS = `pkg-config --cflags glib-2.0`
GLIB_LIBS = `pkg-config --libs glib-2.0`

OCIGLUELIB_CFLAGS = -I. $(ORACLE_CFLAGS) $(GLIB_CFLAGS)
OCIGLUELIB_LIBS = $(ORACLE_LIBS) $(GLIB_LIBS) 
OCIGLUELIB_LINKFLAGS = -shared -o $(TARGET) -pthread $(OCIGLUELIB_LIBS) 

SOURCE_H_FILES = ociglue.h
SOURCE_C_FILES = ociglue.c
                                                                                                                                                             
all: libociglue.so

libociglue.so: ociglue.c ociglue.h
	$(CC) $(OCIGLUELIB_CFLAGS) $(SOURCE_C_FILES) $(OCIGLUELIB_LINKFLAGS)
