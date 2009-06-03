thisdir = class/Mono.C5
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.C5.dll
LIBRARY_SNK = c5.snk
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll -nowarn:169
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

EXTRA_DISTFILES = \
	LICENSE.txt \
	c5.pub c5.snk c5.xml

# This is a .NET 2.0+ only assembly
VALID_PROFILE := $(filter net_2_0 net_4_0, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-Mono.C5.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
