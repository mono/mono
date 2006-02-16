thisdir = class

MONO_POSIX := Mono.Posix
MONO_SYMBOLWRITER := Mono.CompilerServices.SymbolWriter

bootstrap_dirs := \
	corlib					\
	System					\
	System.XML				\
	Mono.Security				\
	System.Security				\
	System.Configuration			\
	System					\
	$(MONO_SYMBOLWRITER)

# Note that Mono.Security and System.Security aren't listed.
# We may have to add those if 'mcs' starts using them.
basic_SUBDIRS := corlib System System.XML System

net_1_1_bootstrap_SUBDIRS := $(bootstrap_dirs) PEAPI
net_2_0_bootstrap_SUBDIRS := $(bootstrap_dirs) PEAPI

net_1_1_java_SUBDIRS = \
	System.Xml					\
	System.Data					\
	System.Drawing					\
	Novell.Directory.Ldap				\
	System.DirectoryServices			\
	System.Runtime.Serialization.Formatters.Soap	\
	System.Runtime.Remoting

common_dirs := \
	corlib				\
	System				\
	System.XML			\
	Mono.Security			\
	System				\
	System.Security			\
	System.Configuration		\
	System				\
	I18N				\
	System.Drawing			\
	System.EnterpriseServices	\
	Mono.Data.Tds			\
	System.Data			\
	Mono.Data			\
	Mono.GetOptions			\
	System.Web			\
	System.Web.Services		\
	System.Web			\
	System.Runtime.Serialization.Formatters.Soap \
	System.Runtime.Remoting		\
	System.Configuration.Install 	\
	System.Management		\
	$(MONO_SYMBOLWRITER)		\
	$(MONO_POSIX)			\
	Mono.Data.SqliteClient		\
	Mono.Data.SybaseClient		\
	Mono.Data.TdsClient		\
	System.Data.OracleClient	\
	Accessibility			\
	Microsoft.VisualC		\
	Cscompmgd			\
	Managed.Windows.Forms		\
	Microsoft.VisualBasic		\
	System.Design			\
	PEAPI				\
	Npgsql				\
	ICSharpCode.SharpZipLib		\
	Compat.ICSharpCode.SharpZipLib	\
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
	IBM.Data.DB2			\
	Microsoft.JScript

default_dirs := FirebirdSql.Data.Firebird

net_2_0_dirs := \
	Microsoft.Build.Framework	\
	Microsoft.Build.Utilities	\
	Microsoft.Build.Engine		\
	Microsoft.Build.Tasks

default_SUBDIRS := $(common_dirs) $(default_dirs)
net_2_0_SUBDIRS := $(common_dirs) $(net_2_0_dirs)

include ../build/rules.make

SUBDIRS = $(common_dirs) $(default_dirs) $(net_2_0_dirs)

# These libraries below are either unimplemented or don't compile, but
# I wrote makefiles for them.

BROKEN_SUBDIRS = \
	Custommarshalers		\
	Mono.Globalization

DIST_ONLY_SUBDIRS = \
	$(BROKEN_SUBDIRS)

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
	notes/BitVecto32.txt	\
	$(monolite_files)

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

basic_files = mcs.exe mscorlib.dll System.dll System.Xml.dll
monolite_files = $(basic_files:%=lib/monolite/%)

DIST_PROFILE_DIRS = basic net_1_1_bootstrap default net_2_0_bootstrap net_2_0
COMPAT_PROFILE_DIRS = default net_2_0

lib/monolite:
	$(MKINSTALLDIRS) $@

$(monolite_files): lib/monolite/%: lib/basic/% lib/monolite
	cp -p $< $@

$(basic_files:%=lib/basic/%):
	cd $(topdir) && $(MAKE) profile-do--basic--all

dist-default: $(monolite_files)
dist-local: dist-default
	-$(MKINSTALLDIRS) $(DIST_PROFILE_DIRS:%=$(distdir)/lib/%) $(COMPAT_PROFILE_DIRS:%=$(distdir)/compat/%)
