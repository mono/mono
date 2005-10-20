thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Security.dll
LIB_MCS_FLAGS = -nowarn:618 \
	-r:$(corlib) -r:System.dll -r:System.Xml.dll \
	-debug+

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:169,219,1595

ifeq (net_2_0,$(PROFILE))
LIB_MCS_FLAGS += -r:Mono.Security.dll -nowarn:414
TEST_MCS_FLAGS += -nowarn:168,183,414
endif

include ../../build/library.make
