thisdir = class/System.Drawing
include ../../build/rules.make
SUBDIRS = 

LIBRARY = System.Drawing.dll

LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll /nowarn:649 /nowarn:169 /codepage:28591

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /r:System.Drawing.dll -nowarn:0618 -nowarn:219 -nowarn:169 -nowarn:1595

include ../../build/library.make
