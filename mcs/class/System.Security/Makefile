thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Security.dll
LIB_MCS_FLAGS = /nowarn:618 \
	/r:$(corlib) /r:System.dll /r:System.Xml.dll /r:Mono.Security.dll \
	/debug+

TEST_MCS_FLAGS = /nowarn:1595 $(LIB_MCS_FLAGS)

include ../../build/library.make
