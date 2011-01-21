thisdir = class/WebMatrix.Data
SUBDIRS = 
include ../../build/rules.make

LIBRARY = WebMatrix.Data.dll

include ../../build/library.make

LIB_MCS_FLAGS += -r:$(corlib) -r:System.dll -r:System.Data.dll
TEST_MCS_FLAGS += -r:System.dll -r:System.Data.dll
