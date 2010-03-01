thisdir = class/Microsoft.Build.Tasks
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Tasks.dll
ifeq (1.0, $(FRAMEWORK_VERSION))

LIBRARY_NAME = dummy-Microsoft.Build.Tasks.dll
NO_INSTALL = yes
NO_TEST = yes
NO_SIGN_ASSEMBLY = yes

else

ifeq (3.5, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v3.5
endif

LIBRARY_NAME = Microsoft.Build.Tasks$(NAME_SUFFIX).dll

endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Core.dll			\
	/r:System.Xml.dll			\
	/r:System.Windows.Forms.dll		\
	/r:Microsoft.Build.Utilities.dll	\
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Engine.dll

TEST_MCS_FLAGS = \
	/r:Microsoft.Build.Engine.dll		\
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll

EXTRA_DISTFILES = \
	Test/resources/test.cs \
	Test/resources/Sample.cs \
	Test/resources/Sample.vb \
	Test/resources/junk.txt

test-local: Test/resources/test.dll

Test/resources/test.dll: Test/resources/test.cs
	$(CSCOMPILE) -target:library Test/resources/test.cs

include ../../build/library.make
