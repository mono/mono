thisdir = class/Mono.C5
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.C5.dll
LIBRARY_SNK = c5.snk
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll -nowarn:169,219,414,1030,3001,3005,3006
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

EXTRA_DISTFILES = \
	LICENSE.txt \
	c5.pub c5.snk c5.xml

include ../../build/library.make
