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

TXT_RESOURCE_STRINGS = ../referencesource/System.Data/system.data.txt
RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Data.Common/src/Resources/Strings.resx	\
	../../../external/corefx/src/System.Data.SqlClient/src/Resources/Strings.resx	\
	../../../external/corefx/src/System.Data.Odbc/src/Resources/Strings.resx

TEST_LIB_REFS = System.Core Mono.Data.Sqlite
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618,169,612,219,168

TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file
XTEST_LIB_REFS = System System.Core System.Xml Facades/System.Text.Encoding.CodePages Facades/System.Threading.Tasks Facades/System.Runtime.InteropServices.RuntimeInformation 

TEST_MONO_PATH = .

EXTRA_DISTFILES = \
	$(wildcard Test/System.Data/*.xml)		\
	$(wildcard Test/System.Data/*.xsd)		\
	$(wildcard Test/System.Xml/*.xml)		\
	$(wildcard Test/System.Xml/*.xsd)		\
	$(wildcard Test/System.Data/schemas/*.xsd)		\
	Test/test-config-file			\
	Test/System.Data/binserialize/*.bin	\
	Test/ProviderTests/sql/*.sql	\
	SqliteTest.db	\
	corefx.common.sources	\
	corefx.unix.sources

include ../../build/library.make
