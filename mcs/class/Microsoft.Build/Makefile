thisdir = class/Microsoft.Build
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.dll

ifneq (4.0, $(FRAMEWORK_VERSION))
LIBRARY_NAME = dummy-Microsoft.Build.dll
NO_INSTALL = yes
NO_TEST = yes
NO_SIGN_ASSEMBLY = yes
endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Core.dll			\
	/r:System.Xml.dll			\
	/r:Microsoft.Build.Engine.dll		\
	/r:Microsoft.Build.Framework.dll

include ../../build/library.make

export TESTING_MONO=a
XBUILD_DIR=../../tools/xbuild
include $(XBUILD_DIR)/xbuild_targets.make
