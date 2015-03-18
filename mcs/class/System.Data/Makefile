thisdir = class/System.Data
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.dll

LIB_REFS = System System.Xml System.Core System.Numerics
LIB_MCS_FLAGS = \
	-nowarn:649 \
	-d:PLATFORM_UNIX \
	-d:USEOFFSET \
	-unsafe

MOBILE := $(filter monotouch monodroid xammac mobile mobile_static, $(PROFILE))
ifdef MOBILE
LIB_REFS += Mono.Data.Tds System.Transactions
LIB_MCS_FLAGS += -d:NO_CODEDOM -d:NO_OLEDB -d:NO_ODBC -d:NO_CONFIGURATION -nowarn:618
BUILT_SOURCES = \
	gen_SqlConnection.cs \
	gen_SqlParameter.cs \
	gen_SqlParameterCollection.cs
else
LIB_REFS += System.EnterpriseServices Mono.Data.Tds System.Configuration System.Transactions
BUILT_SOURCES = \
	gen_OdbcConnection.cs \
	gen_OleDbConnection.cs \
	gen_SqlConnection.cs \
	gen_OdbcParameter.cs \
	gen_OleDbParameter.cs \
	gen_SqlParameter.cs \
	gen_OdbcParameterCollection.cs \
	gen_OleDbParameterCollection.cs \
	gen_SqlParameterCollection.cs
endif

RESOURCE_STRINGS = ../../../external/referencesource/System.Data/system.data.txt

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -r:System.Core.dll -r:Mono.Data.Sqlite.dll -nowarn:618,169,612,219,168

TEST_MONO_PATH = .

EXTRA_DISTFILES = \
	TODO					\
	$(wildcard Test/System.Data/*.xml)		\
	$(wildcard Test/System.Data/*.xsd)		\
	$(wildcard Test/System.Xml/*.xml)		\
	$(wildcard Test/System.Xml/*.xsd)		\
	Test/System.Data/schemas/ChangeLog	\
	$(wildcard Test/System.Data/schemas/*.xsd)		\
	Mono.Data.SqlExpressions/Parser.jay	\
	app_test_net_2_0.config			\
	app_test_net_4_5.config			\
	Test/System.Data/binserialize/*.bin	\
	SqliteTest.db

gen_OdbcConnection.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbConnectionHelper.cs
	sed -e "s/NAMESPACE/System.Data.Odbc/g" $< |\
	sed -e "s/CONNECTIONFACTORYOBJECTNAME/OdbcConnectionFactory.SingletonInstance/g" |\
	sed -e "s/CONNECTIONOBJECTNAME/OdbcConnection/g" >$@
gen_OleDbConnection.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbConnectionHelper.cs
	sed -e s/NAMESPACE/System.Data.OleDb/g $< |\
	sed -e s/CONNECTIONFACTORYOBJECTNAME/OleDbConnectionFactory.SingletonInstance/g |\
	sed -e s/CONNECTIONOBJECTNAME/OleDbConnection/g >$@
gen_SqlConnection.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbConnectionHelper.cs
	sed -e s/NAMESPACE/System.Data.SqlClient/g $< |\
	sed -e s/CONNECTIONFACTORYOBJECTNAME/SqlConnectionFactory.SingletonInstance/g |\
	sed -e s/CONNECTIONOBJECTNAME/SqlConnection/g >$@
gen_OdbcParameter.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbParameterHelper.cs
	sed -e s/RESNAMESPACE/System.Data/g $< |\
	sed -e s/NAMESPACE/System.Data.Odbc/g |\
	sed -e s/PARAMETEROBJECTNAME/OdbcParameter/g >$@
gen_OleDbParameter.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbParameterHelper.cs
	sed -e s/RESNAMESPACE/System.Data/g $< |\
	sed -e s/NAMESPACE/System.Data.OleDb/g |\
	sed -e s/PARAMETEROBJECTNAME/OleDbParameter/g >$@
gen_SqlParameter.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbParameterHelper.cs
	sed -e s/RESNAMESPACE/System.Data/g $< |\
	sed -e s/NAMESPACE/System.Data.SqlClient/g |\
	sed -e s/PARAMETEROBJECTNAME/SqlParameter/g >$@
gen_OdbcParameterCollection.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbParameterCollectionHelper.cs
	sed -e s/RESNAMESPACE/System.Data/g $< |\
	sed -e s/NAMESPACE/System.Data.Odbc/g |\
	sed -e s/PARAMETERCOLLECTIONOBJECTNAME/OdbcParameterCollection/g |\
	sed -e s/PARAMETEROBJECTNAME/OdbcParameter/g >$@
gen_OleDbParameterCollection.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbParameterCollectionHelper.cs
	sed -e s/RESNAMESPACE/System.Data/g $< |\
	sed -e s/NAMESPACE/System.Data.OleDb/g |\
	sed -e s/PARAMETERCOLLECTIONOBJECTNAME/OleDbParameterCollection/g |\
	sed -e s/PARAMETEROBJECTNAME/OleDbParameter/g >$@
gen_SqlParameterCollection.cs: ../../../external/referencesource/System.Data/System/Data/ProviderBase/DbParameterCollectionHelper.cs
	sed -e s/RESNAMESPACE/System.Data/g $< |\
	sed -e s/NAMESPACE/System.Data.SqlClient/g |\
	sed -e s/PARAMETERCOLLECTIONOBJECTNAME/SqlParameterCollection/g |\
	sed -e s/PARAMETEROBJECTNAME/SqlParameter/g >$@

include ../../build/library.make

$(test_lib): $(test_lib).config

$(test_lib).config: app_test_$(PROFILE).config
	cp $< $@
