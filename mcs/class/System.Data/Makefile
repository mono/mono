thisdir = class/System.Data
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.dll

LIB_REFS = System System.Xml System.Core System.Numerics
LIB_MCS_FLAGS = \
	-nowarn:219,414,649,619 \
	-d:COREFX	\
	-d:PLATFORM_UNIX \
	-d:USEOFFSET \
	-d:MONO_PARTIAL_DATA_IMPORT \
	-resource:../../../external/corefx/src/System.Data.SqlClient/src/Resources/System.Data.SqlClient.SqlMetaData.xml	\
	-unsafe

ifdef NO_MONO_SECURITY
MONO_DATA_TDS=
else
MONO_DATA_TDS=Mono.Data.Tds
endif

ifdef MOBILE_PROFILE
LIB_REFS += $(MONO_DATA_TDS) System.Transactions
LIB_MCS_FLAGS += -d:NO_CODEDOM -d:NO_OLEDB -d:NO_ODBC -d:NO_CONFIGURATION
else
LIB_REFS += System.EnterpriseServices $(MONO_DATA_TDS) System.Configuration System.Transactions
endif

TXT_RESOURCE_STRINGS = ../referencesource/System.Data/system.data.txt
RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Data.Common/src/Resources/Strings.resx	\
	../../../external/corefx/src/System.Data.SqlClient/src/Resources/Strings.resx	\
	../../../external/corefx/src/System.Data.Odbc/src/Resources/Strings.resx	

TEST_LIB_REFS = System.Core Mono.Data.Sqlite
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618,169,612,219,168

TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file

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
	corefx.common.sources

gen_OleDbConnection.cs: ../referencesource/System.Data/System/Data/ProviderBase/DbConnectionHelper.cs
	sed -e s/NAMESPACE/System.Data.OleDb/g $< |\
	sed -e s/CONNECTIONFACTORYOBJECTNAME/OleDbConnectionFactory.SingletonInstance/g |\
	sed -e s/CONNECTIONOBJECTNAME/OleDbConnection/g >$@
gen_OleDbParameter.cs: ../referencesource/System.Data/System/Data/ProviderBase/DbParameterHelper.cs
	sed -e s/RESNAMESPACE/System.Data/g $< |\
	sed -e s/NAMESPACE/System.Data.OleDb/g |\
	sed -e s/PARAMETEROBJECTNAME/OleDbParameter/g >$@
gen_OleDbParameterCollection.cs: ../referencesource/System.Data/System/Data/ProviderBase/DbParameterCollectionHelper.cs
	sed -e s/RESNAMESPACE/System.Data/g $< |\
	sed -e s/NAMESPACE/System.Data.OleDb/g |\
	sed -e s/PARAMETERCOLLECTIONOBJECTNAME/OleDbParameterCollection/g |\
	sed -e s/PARAMETEROBJECTNAME/OleDbParameter/g >$@

include ../../build/library.make
