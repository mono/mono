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

ifeq (3.5, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v3.5
else
ifeq (4.0, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v4.0
endif
endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Core.dll		\
	/r:System.Xml.dll			\
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll

TEST_MCS_FLAGS = \
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll

EXTRA_DISTFILES = \
	Test/resources/TestTasks.cs		\
	Test/resources/*.*proj	\
	Test/test-config-file*

Test/resources/TestTasks.dll: Test/resources/TestTasks.cs
	$(CSCOMPILE) Test/resources/TestTasks.cs /r:Microsoft.Build.Framework.dll /r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll /target:library

clean-local: clean-test-tasks

clean-test-tasks:
	rm -f Test/resources/TestTasks.dll	

test-local: copy-config

ifeq (net_4_0, $(PROFILE))
copy-config:
	cp Test/test-config-file-net-4.0 $(test_lib).config
else
ifeq (net_3_5, $(PROFILE))
copy-config:
	cp Test/test-config-file-net-3.5 $(test_lib).config
else
copy-config:
endif
endif

export TESTING_MONO=a
XBUILD_DIR=../../tools/xbuild
include $(XBUILD_DIR)/xbuild_targets.make

test-local: Test/resources/TestTasks.dll

include ../../build/library.make
