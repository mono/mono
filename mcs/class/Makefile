thisdir = class
include ../build/rules.make

MONO_POSIX = Mono.Posix
MONO_CSHARP_DEBUGGER = Mono.CSharp.Debugger

SUBDIRS = \
	corlib				\
	System				\
	I18N				\
	System.XML			\
	System				\
	System.Drawing			\
	System.EnterpriseServices	\
	Mono.Security			\
	Mono.Data.Tds			\
	System.Security			\
	System.Data			\
	System.Data.SqlXml		\
	Mono.GetOptions			\
	System.Web			\
	System.Web.Services		\
	System.Runtime.Serialization.Formatters.Soap \
	System.Runtime.Remoting		\
	System.Configuration.Install 	\
	System.Management		\
	$(MONO_CSHARP_DEBUGGER)         \
	Mono.Data.SqliteClient		\
	Mono.Data.SybaseClient		\
	Mono.Data.TdsClient		\
	System.Data.OracleClient	\
	$(MONO_POSIX)			\
	Accessibility			\
	Microsoft.VisualC		\
	Cscompmgd			\
	System.Windows.Forms		\
	Microsoft.VisualBasic		\        
	System.Design			\
	PEAPI				\
	Npgsql				\
	ICSharpCode.SharpZipLib		\
	ByteFX.Data			\
	Commons.Xml.Relaxng		\
	Novell.Directory.Ldap		\
	Mono.Security.Win32		\
	System.DirectoryServices	\
	System.Messaging	        \
	System.ServiceProcess           \
	System.Drawing.Design		\
	Mono.Http			\
	Microsoft.Vsa			\
	Mono.Cairo                      \
	IBM.Data.DB2			     
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
	Custommarshalers		\
	Mono.Globalization

# No new makefiles for: System.Messaging, System.Web.Mobile,
# System.ServiceProcess

DISTFILES = \
	LICENSE 		\
	README			\
	doc/API-notes		\
	doc/NUnitGuidelines	\
	doc/notes		\
	notes/BitVecto32.txt

all-local install-local test-local run-test-local clean-local uninstall-local:

dist-local: dist-default
	-mkdir $(distdir)/lib
