thisdir = class/I18N/West
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = I18N.West.dll
LIB_REFS = I18N
KEYFILE = ../../mono.pub
LOCAL_MCS_FLAGS = /unsafe
#NO_TEST = yes

EXTRA_DISTFILES = $(wildcard *.ucm) \
	$(wildcard Test/texts/*.txt)

include ../../../build/library.make
