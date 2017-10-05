thisdir = class/Mono.Cairo
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Cairo.dll
KEYFILE = ../mono.pub
#LIB_MCS_FLAGS = -d:CAIRO_1_2
NO_TEST = yes

EXTRA_DISTFILES = \
	Samples/gtk/*.cs \
	Samples/gtk/*.sh \
	Samples/gtk/data/* \
	Samples/png/*.cs \
	Samples/png/*.sh \
	Samples/png/data/* \
	Samples/win32/*.cs \
	Samples/win32/*.sh \
	Samples/x11/*.cs \
	Samples/x11/*.sh \
	Samples/x11/data/*

include ../../build/library.make
