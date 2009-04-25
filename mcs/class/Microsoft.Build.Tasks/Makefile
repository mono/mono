thisdir = class/Microsoft.Build.Tasks
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Tasks.dll
ifeq (1.0, $(FRAMEWORK_VERSION))
LIBRARY_NAME = dummy-Microsoft.Build.Tasks.dll
NO_INSTALL = yes
NO_TEST = yes
NO_SIGN_ASSEMBLY = yes
endif

ifeq (3.5, $(FRAMEWORK_VERSION))
LIBRARY_NAME = Microsoft.Build.Tasks.v3.5.dll
endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Xml.dll				\
	/r:System.Windows.Forms.dll		\
	/r:Microsoft.Build.Utilities.dll	\
	/r:Microsoft.Build.Framework.dll

TEST_MCS_FLAGS = \
	/r:Microsoft.Build.Engine.dll		\
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Tasks.dll		\
	/r:Microsoft.Build.Utilities.dll

EXTRA_DISTFILES = Test/resources/test.cs Test/resources/Sample.cs

test-local: Test/resources/test.dll

Test/resources/test.dll: Test/resources/test.cs
	$(CSCOMPILE) -target:library Test/resources/test.cs

include ../../build/library.make
