thisdir = class
SUBDIRS = \
	corlib				\
	I18N				\
	System				\
	System.XML			\
	System.Drawing			\
	System.EnterpriseServices	\
	Mono.Data.Tds			\
	System.Security			\
	System.Data			\
	Mono.GetOptions			\
	System.Web			\
	System.Web.Services		\
	System.Runtime.Serialization.Formatters.Soap \
	System.Runtime.Remoting		\
	System.Configuration.Install 	\
	System.Management		\
	Mono.CSharp.Debugger		\
	Mono.Data.DB2Client		\
	Mono.Data.MySql			\
	Mono.Data.PostgreSqlClient	\
	Mono.Data.SqliteClient		\
	Mono.Data.SybaseClient		\
	Mono.Data.TdsClient		\
	System.Data.OracleClient	\
	Mono.PEToolkit			\
	Mono.Posix			\
	Accessibility			\
	Microsoft.VisualBasic		\
	Microsoft.VisualC		\
	Cscompmgd			\
	System.Windows.Forms		\
	System.Design			\
	PEAPI				\
	Mono.Security			\
	Npgsql				\
	ICSharpCode.SharpZipLib		\
	ByteFX.Data			\
	Commons.Xml.Relaxng		\
	Mono.Directory.LDAP		\
	Mono.Security.Win32		\
	System.DirectoryServices	\
#	Microsoft.Vsa			\
#	Microsoft.JScript		\

# Starting with Microsoft.Vsa, the libraries above weren't in the
# Linux makefile.gnu before, but they build fine, so might as well
# include them ...
#
# Need a way to only build/test Mono.Security.Win32 on Windows.
# It builds on linux, but make run-test will barf because a
# DLL doesn't exist.

# These libraries below are either unimplemented or don't compile, but
# I wrote makefiles for them.

DIST_ONLY_SUBDIRS = \
	System.Drawing.Design		\
	Custommarshalers		\
	Mono.Globalization

# No new makefiles for: System.Messaging, System.Web.Mobile,
# System.ServiceProcess

include ../build/rules.make

DISTFILES = \
	LICENSE 		\
	README			\
	doc/API-notes		\
	doc/NUnitGuidelines	\
	doc/notes		\
	notes/BitVecto32.txt

all-local install-local test-local run-test-local clean-local:

dist-local: dist-default
	-mkdir $(distdir)/lib
