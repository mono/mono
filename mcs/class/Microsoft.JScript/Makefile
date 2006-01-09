thisdir = class/Microsoft.JScript
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.JScript.dll
LIB_MCS_FLAGS = -r:System.dll -r:Microsoft.Vsa.dll -debug+

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

include ../../build/library.make
