thisdir = class/FirebirdSql.Data.Firebird
SUBDIRS =
include ../../build/rules.make

LIBRARY = FirebirdSql.Data.Firebird.dll
LIBRARY_SNK = FirebirdSql.Data.Firebird.snk

LIB_DEFINE_FLAGS = 

ifeq ($(PLATFORM), linux)
LIB_DEFINE_FLAGS = /d:LINUX
endif

LIB_MCS_FLAGS = \
	$(LIB_DEFINE_FLAGS) \
	/r:$(corlib) /r:System.dll \
	/r:System.Xml.dll /r:System.Data.dll \
	/r:System.Drawing \
	@FirebirdSql.Data.Firebird.dll.resources

TEST_MCS_FLAGS = /r:$(corlib) /r:System.dll \
	/r:System.Xml.dll /r:System.Data.dll \

EXTRA_DISTFILES = \
	license.txt \
	license.html \
	readme.txt \
	changelog.txt \
	FirebirdSql.Data.Firebird.snk \
	FirebirdSql.Data.Firebird.dll.resources \
	FirebirdSql.Data.Common/Resources/isc_error_msg.resources \
	FirebirdSql.Data.Firebird.dll.resources \
	Test/App.config \
	Test/AssemblyInfo.cs \
	Test/BaseTest.cs \
	Test/FbArrayTest.cs \
	Test/FbBlobTest.cs \
	Test/FbCommandBuilderTest.cs \
	Test/FbCommandTest.cs \
	Test/FbConnectionTest.cs \
	Test/FbDataAdapterTest.cs \
	Test/FbDatabaseInfoTest.cs \
	Test/FbDatabaseSchemaTest.cs \
	Test/FbDataReaderTest.cs \
	Test/FbImplicitTransactionTest.cs \
	Test/FbParameterCollectionTest.cs \
	Test/FbParameterTest.cs \
	Test/FbScriptTest.cs \
	Test/FbServicesTests.cs \
	Test/FbStoredProcCallsTest.cs \
	Test/FbTransactionTest.cs \
	Test/GuidTest.cs \

include ../../build/library.make

