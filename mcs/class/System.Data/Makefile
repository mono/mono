thisdir = class/System.Data
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.dll

LIB_REFS = System System.Xml System.Core System.Numerics System.Transactions
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS = \
	-nowarn:219,414,649,619 \
	-d:COREFX	\
	-d:PLATFORM_UNIX \
	-d:USEOFFSET \
	-d:MONO_PARTIAL_DATA_IMPORT \
	-resource:../../../external/corefx/src/System.Data.SqlClient/src/Resources/System.Data.SqlClient.SqlMetaData.xml	\
	-unsafe

ifdef MOBILE_PROFILE
LIB_MCS_FLAGS += -d:NO_CODEDOM -d:NO_OLEDB -d:NO_ODBC -d:NO_CONFIGURATION
else
LIB_REFS += System.EnterpriseServices System.Configuration
endif

ifdef MONO_FEATURE_APPLETLS
LIB_MCS_FLAGS += -d:MONO_FEATURE_APPLETLS
endif

ifdef ENABLE_GSS
LIB_MCS_FLAGS += -d:ENABLE_GSS
endif

ifndef PROFILE_DISABLE_BTLS
ifdef HAVE_BTLS
LIB_MCS_FLAGS += -d:MONO_FEATURE_BTLS
endif
endif

TXT_RESOURCE_STRINGS = ../referencesource/System.Data/system.data.txt
RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Data.Common/src/Resources/Strings.resx	\
	../../../external/corefx/src/System.Data.SqlClient/src/Resources/Strings.resx	\
	../../../external/corefx/src/System.Data.Odbc/src/Resources/Strings.resx

TEST_RESOURCE_FILES = \
	$(wildcard Test/System.Data/*.xml)		\
	$(wildcard Test/System.Data/*.xsd)		\
	$(wildcard Test/System.Xml/*.xml)		\
	$(wildcard Test/System.Xml/*.xsd)		\
	$(wildcard Test/System.Data/schemas/*.xml)		\
	$(wildcard Test/System.Data/schemas/*.xsd)		\
	$(wildcard Test/System.Data/binserialize/*.bin)	\
	$(wildcard Test/ProviderTests/sql/*.sql)	\
	Test/Mono.Data.SqlExpressions/dateComparisonTest.xml	\
	Test/System.Data.Common/SqliteTest.db

TEST_LIB_REFS = System.Core Mono.Data.Sqlite
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618,169,612,219,168 $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file

USE_XTEST_REMOTE_EXECUTOR = YES
XTEST_LIB_REFS = System System.Core System.Xml Facades/System.Text.Encoding.CodePages Facades/System.Threading.Tasks Facades/System.Runtime.InteropServices.RuntimeInformation Facades/System.Text.RegularExpressions

EXTRA_DISTFILES = \
	$(TEST_RESOURCE_FILES)	\
	Test/test-config-file

include ../../build/library.make
