thisdir = class/Microsoft.Build
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.dll

LIB_REFS = $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core $(PARENT_PROFILE)System.Xml Microsoft.Build.Engine Microsoft.Build.Framework
LIB_MCS_FLAGS = \
	/d:MICROSOFT_BUILD_DLL
	
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)
TEST_LIB_REFS = $(XBUILD_UTILITIES)

EXTRA_DISTFILES = \
	Microsoft.Build.Internal/ExpressionParser.jay	\
	Test/FunctionalTestReferenceProject.csproj	\
	Test/FunctionalTestReferenceProject3.csproj	\
	Test/Microsoft.Build.Test.csproj	\
	Test/Microsoft.Build.csproj

EXPR_PARSER = Microsoft.Build.Internal/ExpressionParser

$(EXPR_PARSER).cs: $(EXPR_PARSER).jay $(topdir)/jay/skeleton.cs
	(cd Microsoft.Build.Internal; $(topdir)/../jay/jay -ctv < $(topdir)/../jay/skeleton.cs ExpressionParser.jay > ExpressionParser.cs)

BUILT_SOURCES = $(EXPR_PARSER).cs

include ../../build/library.make

include $(XBUILD_DIR)/xbuild_test.make
