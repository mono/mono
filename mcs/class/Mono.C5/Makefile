thisdir = class/Mono.C5
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.C5.dll
LIBRARY_SNK = c5.snk
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll
NO_TEST = yes

EXTRA_DISTFILES = \
	LICENSE.txt \
	c5.pub c5.snk c5.xml

include ../../build/library.make
