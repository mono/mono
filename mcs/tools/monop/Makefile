thisdir = tools/monop
SUBDIRS = 
include ../../build/rules.make

PROGRAM = monop.exe
LOCAL_MCS_FLAGS += -d:NO_AUTHENTICODE,STATIC,NO_SYMBOL_WRITER

CLEAN_FILES = monop.exe monop2.exe *.mdb

include ../../build/executable.make
