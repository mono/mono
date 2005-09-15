thisdir = class/Microsoft.JScript
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.JScript.dll

ifeq (net_2_0, $(PROFILE))
COMPILER = gmcs.exe
else
COMPILER = mcs.exe
endif

LIB_MCS_FLAGS = -r:System.dll -r:Microsoft.Vsa.dll -r:$(topdir)/class/lib/$(PROFILE)/$(COMPILER) -debug+

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

include ../../build/library.make
