thisdir = class/Microsoft.Build
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.dll

LIB_REFS = $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core $(PARENT_PROFILE)System.Xml Microsoft.Build.Engine Microsoft.Build.Framework
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = \
	/d:MICROSOFT_BUILD_DLL

TEST_RESOURCE_FILES = \
	Test/resources/FunctionalTestReferenceProject.csproj	\
	Test/resources/FunctionalTestReferenceProject3.csproj	\
	Test/resources/Microsoft.Build.Test.csproj	\
	Test/resources/Microsoft.Build.csproj

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))
TEST_LIB_REFS = $(XBUILD_UTILITIES)
TEST_NUNITLITE_APP_CONFIG_RUNTIME=Test/test-config-file-$(PROFILE)

EXTRA_DISTFILES = \
	Microsoft.Build.Internal/ExpressionParser.jay	\
	$(TEST_RESOURCE_FILES)

EXPR_PARSER = Microsoft.Build.Internal/ExpressionParser

$(EXPR_PARSER).cs: $(EXPR_PARSER).jay $(topdir)/jay/skeleton.cs
	(cd Microsoft.Build.Internal; $(topdir)/../jay/jay -ctv -o ExpressionParser.cs ExpressionParser.jay < $(topdir)/../jay/skeleton.cs )

BUILT_SOURCES = $(EXPR_PARSER).cs

include ../../build/library.make

include $(XBUILD_DIR)/xbuild_test.make
