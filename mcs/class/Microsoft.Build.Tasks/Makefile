thisdir = class/Microsoft.Build.Tasks
SUBDIRS =
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Tasks.dll

LIBRARY_NAME = Microsoft.Build.Tasks$(NAME_SUFFIX).dll

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Core.dll			\
	/r:System.Xml.dll			\
	/r:System.Windows.Forms.dll		\
	/r:$(XBUILD_UTILITIES)			\
	/r:$(XBUILD_FRAMEWORK)			\
	/r:$(XBUILD_ENGINE)			\
	/r:$(XBUILD_TASKS)

TEST_MCS_FLAGS = \
	/r:System.Xml.dll			\
	/r:$(XBUILD_ENGINE)	\
	/r:$(XBUILD_FRAMEWORK)	\
	/r:$(XBUILD_TASKS)	\
	/r:$(XBUILD_UTILITIES)			\
	/r:System.Core.dll

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
	TEST_MCS_FLAGS += /r:Microsoft.Build.dll
endif

EXTRA_DISTFILES = \
	Test/resources/test.cs \
	Test/resources/Sample.cs \
	Test/resources/Sample.vb \
	Test/resources/junk.txt \
	Test/test-config-file*

Test/resources/test.dll: Test/resources/test.cs
	$(CSCOMPILE) -target:library Test/resources/test.cs

clean-test-resources:
	rm -f Test/resources/test.dll

test-local: Test/resources/test.dll

clean-local: clean-test-resources

include $(XBUILD_DIR)/xbuild_test.make
include ../../build/library.make
