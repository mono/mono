thisdir = class/Npgsql
SUBDIRS = 

OVERRIDE_BARE_TARGETS = 1


include ../../build/rules.make

LIBRARY = Npgsql.dll



LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Design.dll /r:Lib/System.Security.Tls.dll -resource:Npgsql/NpgsqlConnection.bmp -resource:Npgsql/NpgsqlCommand.bmp -resource:Npgsql/NpgsqlDataAdapter.bmp -resource:Npgsql/NpgsqlState.resources -resource:Npgsql/NpgsqlClosedState.resources -resource:Npgsql/NpgsqlParameterCollection.resources -resource:Npgsql/NpgsqlParameter.resources -resource:Npgsql/NpgsqlCommand.resources -resource:Npgsql/NpgsqlConnectedState.resources -resource:Npgsql/NpgsqlConnection.resources -resource:Npgsql/NpgsqlReadyState.resources -resource:Npgsql/NpgsqlTransaction.resources -resource:Npgsql/NpgsqlEventLog.resources -resource:Npgsql/NpgsqlException.resources -resource:Npgsql/PGUtil.resources -resource:NpgsqlTypes/NpgsqlTypesHelper.resources -resource:Npgsql/Design/ConnectionStringEditorForm.resources
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

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
	Npgsql/Npgsql.xml

include ../../build/library.make

all: resgen $(the_lib)

resgen:
	monoresgen Npgsql/NpgsqlState.resx
	monoresgen Npgsql/NpgsqlClosedState.resx
	monoresgen Npgsql/NpgsqlParameterCollection.resx
	monoresgen Npgsql/NpgsqlCommand.resx
	monoresgen Npgsql/NpgsqlConnectedState.resx
	monoresgen Npgsql/NpgsqlParameter.resx
	monoresgen Npgsql/NpgsqlConnection.resx
	monoresgen Npgsql/NpgsqlReadyState.resx
	monoresgen Npgsql/NpgsqlTransaction.resx
	monoresgen Npgsql/NpgsqlEventLog.resx
	monoresgen Npgsql/NpgsqlException.resx
	monoresgen Npgsql/PGUtil.resx
	monoresgen NpgsqlTypes/NpgsqlTypesHelper.resx
	monoresgen Npgsql/Design/ConnectionStringEditorForm.resx

		

