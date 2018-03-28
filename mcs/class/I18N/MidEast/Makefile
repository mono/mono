thisdir = class/I18N/MidEast
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = I18N.MidEast.dll
LIB_REFS = I18N
KEYFILE = ../../mono.pub
LOCAL_MCS_FLAGS = /unsafe
TEST_LIB_REFS = I18N

EXTRA_DISTFILES = $(wildcard *.ucm) \
	$(wildcard Test/texts/*.txt)

include ../../../build/library.make
