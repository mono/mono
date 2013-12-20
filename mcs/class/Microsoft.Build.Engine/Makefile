thisdir = class/Microsoft.Build.Engine
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Engine.dll

LIB_MCS_FLAGS = \
	/r:$(corlib)		\
	/r:System.dll		\
	/r:System.Core.dll	\
	/r:System.Xml.dll	\
	/r:$(XBUILD_FRAMEWORK)	\
	/r:$(XBUILD_UTILITIES)

TEST_MCS_FLAGS = \
	/r:$(XBUILD_FRAMEWORK)	\
	/r:$(XBUILD_UTILITIES) \
	/r:System.Xml.dll

EXTRA_DISTFILES = \
	Test/resources/TestTasks.cs		\
	Test/resources/*.*proj	\
	Test/resources/*.csproj \
	Test/test-config-file*

Test/resources/TestTasks.dll: Test/resources/TestTasks.cs
	$(CSCOMPILE) Test/resources/TestTasks.cs /r:$(XBUILD_FRAMEWORK) /r:$(XBUILD_UTILITIES) /target:library

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
include $(XBUILD_DIR)/xbuild_targets.make

test-local: Test/resources/TestTasks.dll

include ../../build/library.make
