thisdir = class/Cscompmgd
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Cscompmgd.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169
LIBRARY_NAME = cscompmgd.dll

include ../../build/library.make
