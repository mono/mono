thisdir = class/Npgsql
SUBDIRS = 

OVERRIDE_BARE_TARGETS = 1


include ../../build/rules.make

LIBRARY = Npgsql.dll


LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
		/r:System.Data.dll /r:System.Drawing.dll \
		/r:System.Windows.Forms.dll /r:System.Design.dll \
		/r:../Mono.Security.Tls/System.Security.Tls.dll \
		-resource:Npgsql/NpgsqlConnection.bmp \
		-resource:Npgsql/NpgsqlCommand.bmp \
		-resource:Npgsql/NpgsqlDataAdapter.bmp \
		-resource:Npgsql/NpgsqlState.resources \
		-resource:Npgsql/NpgsqlClosedState.resources \
		-resource:Npgsql/NpgsqlParameterCollection.resources \
		-resource:Npgsql/NpgsqlParameter.resources \
		-resource:Npgsql/NpgsqlCommand.resources \
		-resource:Npgsql/NpgsqlConnectedState.resources \
		-resource:Npgsql/NpgsqlConnection.resources \
		-resource:Npgsql/NpgsqlReadyState.resources \
		-resource:Npgsql/NpgsqlTransaction.resources \
		-resource:Npgsql/NpgsqlEventLog.resources \
		-resource:Npgsql/NpgsqlException.resources \
		-resource:Npgsql/PGUtil.resources \
		-resource:NpgsqlTypes/NpgsqlTypesHelper.resources \
		-resource:Npgsql/Design/ConnectionStringEditorForm.resources

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

RESX_RES = Npgsql/NpgsqlState.resources Npgsql/NpgsqlClosedState.resources Npgsql/NpgsqlParameterCollection.resources Npgsql/NpgsqlCommand.resources Npgsql/NpgsqlConnectedState.resources Npgsql/NpgsqlParameter.resources Npgsql/NpgsqlConnection.resources Npgsql/NpgsqlReadyState.resources Npgsql/NpgsqlTransaction.resources Npgsql/NpgsqlEventLog.resources Npgsql/NpgsqlException.resources Npgsql/PGUtil.resources NpgsqlTypes/NpgsqlTypesHelper.resources Npgsql/Design/ConnectionStringEditorForm.resources

all: $(RESX_RES) $(the_lib)

%.resources: %.resx
	monoresgen $<

clean: clean-recursive clean-local

install: install-recursive install-local
