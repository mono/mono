thisdir = class/Microsoft.Build.Tasks
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Tasks.dll
BUILD_FRAMEWORK = Microsoft.Build.Framework.dll
BUILD_ENGINE = Microsoft.Build.Engine.dll

ifeq (3.5, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v3.5
BUILD_FRAMEWORK := $(topdir)/class/lib/$(PROFILE)/$(BUILD_FRAMEWORK)
BUILD_ENGINE := $(topdir)/class/lib/$(PROFILE)/$(BUILD_ENGINE)
else
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
NAME_SUFFIX = .v4.0
endif
endif

# Some tests are explicitly testing Microsoft.Build.Tasks.v3.5.dll
TEST_MONO_PATH = $(topdir)/class/lib/net_3_5$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_2_0

LIBRARY_NAME = Microsoft.Build.Tasks$(NAME_SUFFIX).dll

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Core.dll			\
	/r:System.Xml.dll			\
	/r:System.Windows.Forms.dll		\
	/r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll	\
	/r:$(BUILD_FRAMEWORK)	\
	/r:$(BUILD_ENGINE)

TEST_MCS_FLAGS = \
	/r:$(BUILD_ENGINE)	\
	/r:$(BUILD_FRAMEWORK)	\
	/r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll \
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
XBUILD_DIR=../../tools/xbuild
include $(XBUILD_DIR)/xbuild_targets.make

include ../../build/library.make
