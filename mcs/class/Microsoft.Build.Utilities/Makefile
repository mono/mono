thisdir = class/Microsoft.Build.Utilities
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Utilities.dll
ifeq (1.0, $(FRAMEWORK_VERSION))
LIBRARY_NAME = dummy-Microsoft.Build.Utilities.dll
NO_INSTALL = yes
NO_TEST = yes
NO_SIGN_ASSEMBLY = yes
endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:Microsoft.Build.Framework.dll

TEST_MCS_FLAGS = /r:Microsoft.Build.Framework.dll

include ../../build/library.make
