thisdir = class/Npgsql
SUBDIRS = 

include ../../build/rules.make

LIBRARY = Npgsql.dll
LIBRARY_SNK = Npgsql/Npgsql.snk

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
		/r:System.Data.dll  \
		/r:Mono.Security.dll \
		-warn:1 @Npgsql.dll.resources

TEST_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
		 /r:System.Web.dll /r:System.Data.dll \
		-nowarn:0618 -nowarn:219 -nowarn:169

LIB_MCS_FLAGS_RESOURCES = $(LIB_MCS_FLAGS) 

# TODO: We need System.Web.UI.WebControls to build DataAdapterTests.cs
# and DataReaderTests.cs

EXTRA_DISTFILES = \
	LICENSE.txt		\
	README.txt		\
	RELEASENOTES.txt	\
	STATUS.txt		\
	TODO.txt		\
	Npgsql/Npgsql.snk	\
	Npgsql/Npgsql.pub	\
	$(RESX_RES:.resources=.resx) \
	$(OTHER_RES)		\
	Npgsql.dll.resources

CLEAN_FILES = $(RESX_RES)

OTHER_RES = \
	Npgsql/NpgsqlConnection.bmp		\
	Npgsql/NpgsqlCommand.bmp		\
	Npgsql/NpgsqlDataAdapter.bmp

RESX_RES = \
	Npgsql/NpgsqlState.resources				\
	Npgsql/NpgsqlClosedState.resources			\
	Npgsql/NpgsqlParameterCollection.resources		\
	Npgsql/NpgsqlCommand.resources				\
	Npgsql/NpgsqlCommandBuilder.resources			\
	Npgsql/NpgsqlConnectedState.resources			\
	Npgsql/NpgsqlParameter.resources			\
	Npgsql/NpgsqlConnection.resources			\
	Npgsql/NpgsqlReadyState.resources			\
	Npgsql/NpgsqlTransaction.resources			\
	Npgsql/NpgsqlEventLog.resources				\
	Npgsql/NpgsqlException.resources			\
	Npgsql/PGUtil.resources					\
	Npgsql/NpgsqlConnectionString.resources			\
	NpgsqlTypes/NpgsqlTypesHelper.resources			

include ../../build/library.make

$(the_lib): $(RESX_RES) $(OTHER_RES)

$(RESX_RES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
