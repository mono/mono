thisdir = class

MONO_POSIX := Mono.Posix
MONO_CSHARP_DEBUGGER := Mono.CSharp.Debugger

bootstrap_dirs := \
	corlib 					\
	System					\
	System.XML				\
	System					\
	$(MONO_CSHARP_DEBUGGER)

basic_SUBDIRS := corlib System System.XML System
net_1_1_bootstrap_SUBDIRS := $(bootstrap_dirs) Mono.Security
net_2_0_bootstrap_SUBDIRS := $(bootstrap_dirs)

include ../build/rules.make

# For now, Mono.Security.Win32 is built on Linux too.
# However, it isn't tested on Linux, since it depends on a Win32 dll.
SUBDIRS = \
	corlib				\
	System				\
	I18N				\
	System.XML			\
	System				\
	System.Drawing			\
	System.EnterpriseServices	\
	Mono.C5				\
	Mono.Security			\
	Mono.Data.Tds			\
	System.Security			\
	System.Data			\
	Mono.Data			\
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

# These libraries below are either unimplemented or don't compile, but
# I wrote makefiles for them.

# Managed.Windows.Forms will move to the main SUBDIRS soon.

DIST_ONLY_SUBDIRS = \
	Custommarshalers		\
	Mono.Globalization		\
	Managed.Windows.Forms

# No new makefiles for: System.Messaging, System.Web.Mobile,
# System.ServiceProcess

DISTFILES = \
	ecma.pub		\
	mono.pub		\
	msfinal.pub		\
	mono.snk		\
	LICENSE 		\
	README			\
	doc/API-notes		\
	doc/NUnitGuidelines	\
	doc/TemplateTest.cs	\
	notes/BitVecto32.txt

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

basic_files = mcs.exe mscorlib.dll System.dll System.Xml.dll

DIST_PROFILE_DIRS = basic net_1_1_bootstrap default net_2_0_bootstrap net_2_0

lib/basic.tar.gz: $(basic_files:%=lib/basic/%)
	cd lib && { tar cvf - $(basic_files:%=basic/%) | gzip > basic.tar.gz; }

dist-local: dist-default lib/basic.tar.gz
	-$(MKINSTALLDIRS) $(DIST_PROFILE_DIRS:%=$(distdir)/lib/%)
	cp lib/basic.tar.gz $(distdir)/lib
