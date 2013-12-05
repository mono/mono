thisdir = class/Microsoft.Build
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.dll

ifneq (4, $(FRAMEWORK_VERSION_MAJOR))
LIBRARY_NAME = dummy-Microsoft.Build.dll
NO_INSTALL = yes
NO_TEST = yes
NO_SIGN_ASSEMBLY = yes
endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Core.dll			\
	/r:System.Xml.dll			\
	/r:Microsoft.Build.Engine.dll		\
	/r:Microsoft.Build.Framework.dll	\
	/d:MICROSOFT_BUILD_DLL
	
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

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

export TESTING_MONO=a
XBUILD_DIR=../../tools/xbuild
XBUILD_FRAMEWORK_FOLDERS_PATH=xbuild-testing
include $(XBUILD_DIR)/xbuild_targets.make
