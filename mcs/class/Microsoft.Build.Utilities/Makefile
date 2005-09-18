thisdir = class/Microsoft.Build.Utilities
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Utilities.dll
LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:Microsoft.Build.Framework.dll

TEST_MCS_FLAGS = /r:Microsoft.Build.Framework.dll

include ../../build/library.make
