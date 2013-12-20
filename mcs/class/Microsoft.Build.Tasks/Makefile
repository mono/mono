thisdir = class/Microsoft.Build.Tasks
SUBDIRS =
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Tasks.dll

# Some tests are explicitly testing Microsoft.Build.Tasks.v3.5.dll
TEST_MONO_PATH = $(topdir)/class/lib/net_3_5$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_2_0

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
	/r:$(XBUILD_ENGINE)	\
	/r:$(XBUILD_FRAMEWORK)	\
	/r:$(XBUILD_TASKS)	\
	/r:$(XBUILD_UTILITIES)			\
	/r:System.Core.dll

EXTRA_DISTFILES = \
	Test/resources/test.cs \
	Test/resources/Sample.cs \
	Test/resources/Sample.vb \
	Test/resources/junk.txt \
	Test/test-config-file*

test-local: Test/resources/test.dll

Test/resources/test.dll: Test/resources/test.cs
	$(CSCOMPILE) -target:library Test/resources/test.cs

clean-local: clean-test-dll

clean-test-dll:
	rm -f Test/resources/test.dll

test-local: copy-config

ifeq (net_4_5, $(PROFILE))
copy-config:
	cp Test/test-config-file-net-4.0 $(test_lib).config
else
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
endif

export TESTING_MONO=a
include $(XBUILD_DIR)/xbuild_targets.make

include ../../build/library.make
