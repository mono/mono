thisdir = class/Npgsql
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Npgsql.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll /r:System.Data.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

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
