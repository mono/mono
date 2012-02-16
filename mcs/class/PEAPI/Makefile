thisdir = class/PEAPI
SUBDIRS = 
include ../../build/rules.make

LIBRARY = PEAPI.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll -nowarn:414,618
NO_TEST = yes

EXTRA_DISTFILES = README.txt

include ../../build/library.make
