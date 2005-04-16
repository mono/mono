thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Security.dll
LIB_MCS_FLAGS = /nowarn:618 \
	/r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/debug+

ifeq (net_2_0,$(PROFILE))
LIB_MCS_FLAGS += /r:Mono.Security.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 -nowarn:1595

include ../../build/library.make
