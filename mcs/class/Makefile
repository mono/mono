thisdir = class

bootstrap_dirs := \
	corlib					\
	Mono.CompilerServices.SymbolWriter	\
	System					\
	System.XML				\
	Mono.Security				\
	System.Security				\
	System.Configuration			\
	System


# Note that Mono.Security and System.Security aren't listed.
# We may have to add those if 'mcs' starts using them.
basic_SUBDIRS := corlib System System.XML System Mono.Security Mono.CompilerServices.SymbolWriter

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
	Mono.CompilerServices.SymbolWriter \
	Mono.Posix			\
	System.Core			\
	Mono.Security			\
	System				\
	System.Security			\
	System.Configuration		\
	System				\
	System.XML			\
	I18N				\
	System.Drawing			\
	System.Transactions		\
	System.EnterpriseServices	\
	Mono.Data.Tds			\
	System.Data			\
	Mono.Data			\
	Mono.GetOptions			\
	Accessibility			\
	Mono.WebBrowser			\
	Managed.Windows.Forms		\
	Mono.Data.Sqlite		\
	Mono.Data.SqliteClient		\
	System.Web			\
	Mono.Web			\
	System.Web.Services		\
	System.Web			\
	System.Runtime.Serialization.Formatters.Soap \
	System.Runtime.Remoting		\
	System.Configuration.Install 	\
	System.Management		\
	Mono.Data.SybaseClient		\
	Mono.Data.TdsClient		\
	System.Data.OracleClient	\
	Microsoft.VisualC		\
	Cscompmgd			\
	System.Design			\
	PEAPI				\
	Npgsql				\
	Compat.ICSharpCode.SharpZipLib	\
	ICSharpCode.SharpZipLib		\
	Compat.ICSharpCode.SharpZipLib	\
	ByteFX.Data			\
	Commons.Xml.Relaxng		\
	Novell.Directory.Ldap		\
	Mono.Security.Win32		\
	System.DirectoryServices	\
	RabbitMQ.Client			\
	Mono.Messaging			\
	System.Messaging	        \
	Mono.Messaging.RabbitMQ		\
	System.ServiceProcess           \
	System.Drawing.Design		\
	System.Design			\
	Mono.Http			\
	Microsoft.Vsa			\
	Mono.Cairo                      \
	IBM.Data.DB2			\
	Microsoft.JScript		\
	CustomMarshalers		\
	OpenSystem.C			\
	Mono.Cecil			\
	Mono.Cecil.Mdb			\
	SystemWebTestShim

net_1_1_dirs :=		\
	FirebirdSql.Data.Firebird

net_2_0_dirs := \
	Microsoft.Build.Framework	\
	Microsoft.Build.Utilities	\
	Microsoft.Build.Engine		\
	Microsoft.Build.Tasks		\
	System.Xml.Linq			\
	System.Runtime.Serialization	\
	System.Data.DataSetExtensions	\
	System.Data.Linq		\
	System.Web			\
	System.Web.Abstractions		\
	System.Web.Routing		\
	System.IdentityModel		\
	System.IdentityModel.Selectors	\
	System.ServiceModel		\
	System.Web.Extensions		\
	System.Web.Extensions.Design	\
	System.ComponentModel.DataAnnotations	\
	System.Web.DynamicData		\
	System.ServiceModel.Web		\
	System.Web.Mvc			\
	Mono.C5				\
	Mono.Management			\
	Mono.Options 			\
	Mono.Simd			\
	Mono.Tasklets       		\
	WindowsBase

net_2_0_only_dirs := \
	System.Web.Extensions_1.0	\
	System.Web.Extensions.Design_1.0

net_2_1_dirs := \
	corlib			\
	Mono.CompilerServices.SymbolWriter \
	System			\
	System.Net		\
	System.XML		\
	System.Core		\
	System.Xml.Linq	\
	System.Runtime.Serialization	\
	System.ServiceModel	\
	System.ServiceModel.Web

monotouch_dirs := \
	corlib	\
	Mono.CompilerServices.SymbolWriter	\
	System	\
	System.XML	\
	Mono.Security	\
	System	\
	I18N		\
	System.Core	\
	System.Json	\
	System.Xml.Linq	\
	System.Runtime.Serialization	\
	System.ServiceModel	\
	System.ServiceModel.Web	\
	System.Web.Services 

net_3_5_dirs := \
	Microsoft.Build.Framework	\
	Microsoft.Build.Utilities	\
	Microsoft.Build.Engine		\
	Microsoft.Build.Tasks		\
	System.Web.Extensions.Design	

net_4_0_dirs := \
	System.Dynamic		\
	Microsoft.CSharp

net_2_0_bootstrap_SUBDIRS := $(bootstrap_dirs)
net_2_0_SUBDIRS := $(common_dirs) $(net_2_0_dirs) $(net_2_0_only_dirs)
net_2_1_bootstrap_SUBDIRS := corlib System Mono.CompilerServices.SymbolWriter
net_2_1_raw_SUBDIRS := $(net_2_1_dirs)
monotouch_SUBDIRS := $(monotouch_dirs)
net_3_5_SUBDIRS := $(net_3_5_dirs)
net_4_0_bootstrap_SUBDIRS := $(bootstrap_dirs) Mono.Posix System.Core
net_4_0_SUBDIRS := $(common_dirs) $(net_2_0_dirs) $(net_4_0_dirs)

include ../build/rules.make

SUBDIRS = $(common_dirs) $(net_1_1_dirs) $(net_2_0_dirs) $(net_2_0_only_dirs) $(net_2_1_dirs) $(net_3_5_dirs) $(net_4_0_dirs)

# These libraries below are either unimplemented or don't compile, but
#BROKEN_SUBDIRS =
#DIST_ONLY_SUBDIRS = $(BROKEN_SUBDIRS)

# No new makefiles for: System.Messaging, System.Web.Mobile,
# System.ServiceProcess

DISTFILES = \
	ecma.pub		\
	winfx.pub		\
	winfx3.pub		\
	mono.pub		\
	msfinal.pub		\
	silverlight.pub		\
	mono.snk		\
	LICENSE 		\
	README			\
	doc/API-notes		\
	doc/NUnitGuidelines	\
	doc/TemplateTest.cs	\
	notes/BitVecto32.txt	\
	MicrosoftAjaxLibrary/License.htm	\
	$(monolite_files)

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

# What is this used for ?

basic_files = gmcs.exe mscorlib.dll System.dll System.Xml.dll Mono.Security.dll
monolite_files = $(basic_files:%=lib/monolite/%)

lib/monolite:
	$(MKINSTALLDIRS) $@

$(monolite_files): | lib/monolite
$(monolite_files): lib/monolite/%: lib/basic/%
	cp -p $< $@

$(basic_files:%=lib/basic/%):
	cd $(topdir) && $(MAKE) profile-do--basic--all

dist-default: $(monolite_files)
dist-local: dist-default

csproj-local:

