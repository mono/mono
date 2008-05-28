thisdir = class/Microsoft.Build.Engine
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Engine.dll

ifeq (1.0, $(FRAMEWORK_VERSION))
LIBRARY_NAME = dummy-Microsoft.Build.Engine.dll
NO_INSTALL = yes
NO_TEST = yes
NO_SIGN_ASSEMBLY = yes
endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Xml.dll			\
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Utilities.dll

TEST_MCS_FLAGS = \
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Utilities.dll

EXTRA_DISTFILES = \
	Test/resources/TestTasks.cs		\
	Test/resources/*.*proj

Test/resources/TestTasks.dll: Test/resources/TestTasks.cs
	$(CSCOMPILE) Test/resources/TestTasks.cs /r:Microsoft.Build.Framework.dll /r:Microsoft.Build.Utilities.dll /target:library

test-local: Test/resources/TestTasks.dll

include ../../build/library.make
