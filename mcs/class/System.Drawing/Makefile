thisdir = class/System.Drawing
include ../../build/rules.make
SUBDIRS = 

LIBRARY = System.Drawing.dll

LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll /nowarn:649 /nowarn:169

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -resource:Test/resources/indexed.png,indexed.png \
	 /r:System.Drawing.dll -nowarn:0618 -nowarn:219 -nowarn:169 -nowarn:1595

ifeq (net_1_1_java, $(PROFILE))
LIB_MCS_FLAGS +=				\
	-r:System.Xml.dll			\
	-r:rt.dll				\
	-r:J2SE.Helpers.dll
endif

include ../../build/library.make

EXTRA_DISTFILES = Test/resources/indexed.png