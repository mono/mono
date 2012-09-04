thisdir = class/Microsoft.Build
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.dll

ifneq (4, $(FRAMEWORK_VERSION_MAJOR))
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
	
TEST_MCS_FLAGS = /r:System.Core.dll

EXTRA_DISTFILES = \
	Test/FunctionalTestReferenceProject.csproj	\
	Test/FunctionalTestReferenceProject3.csproj	\
	Test/Microsoft.Build.Test.csproj	\
	Test/Microsoft.Build.csproj

include ../../build/library.make

export TESTING_MONO=a
XBUILD_DIR=../../tools/xbuild
include $(XBUILD_DIR)/xbuild_targets.make
