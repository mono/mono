thisdir = class/I18N/Other
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = I18N.Other.dll
LOCAL_MCS_FLAGS = /r:$(corlib) /r:I18N.dll /unsafe
NO_TEST = yes

EXTRA_DISTFILES = $(wildcard *.ucm)

include ../../../build/library.make
