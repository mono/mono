thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Security.dll
LIB_MCS_FLAGS = /nowarn:169 /nowarn:618 \
	/r:$(corlib) /r:System.dll /r:System.Xml.dll

TEST_MCS_FLAGS = /nowarn:1595 $(LIB_MCS_FLAGS)

include ../../build/library.make
