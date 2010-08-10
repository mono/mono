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
basic_SUBDIRS := corlib System System.XML System Mono.Security Mono.CompilerServices.SymbolWriter System.Core

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
	Accessibility			\
	Mono.WebBrowser			\
	Managed.Windows.Forms		\
	Mono.Data.Sqlite		\
	System.Web.ApplicationServices	\
	System.Web			\
	Mono.Web			\
	System.Web.Services		\
	System.Web			\
	System.Runtime.Serialization.Formatters.Soap \
	System.Runtime.Remoting		\
	System.Configuration.Install 	\
	System.Management		\
	System.Data.OracleClient	\
	Microsoft.VisualC		\
	Cscompmgd			\
	System.Design			\
	PEAPI				\
	Npgsql				\
	Compat.ICSharpCode.SharpZipLib	\
	ICSharpCode.SharpZipLib		\
	Compat.ICSharpCode.SharpZipLib	\
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
	Mono.Cairo                      \
	IBM.Data.DB2			\
	CustomMarshalers		\
	OpenSystem.C			\
	Mono.Cecil			\
	Mono.Cecil.Mdb			\
	Mono.Debugger.Soft		\
	SystemWebTestShim

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
	System.Web.Mvc2			\
	Mono.C5				\
	Mono.Management			\
	Mono.Options 			\
	Mono.Simd			\
	Mono.Tasklets       		\
	Mono.CSharp			\
	Moonlight.Build.Tasks		\
	WindowsBase			\
	System.Data.Services

net_2_0_only_dirs := \
	System.Web.Extensions_1.0	\
	System.Web.Extensions.Design_1.0

moonlight_dirs := \
	corlib			\
	Mono.CompilerServices.SymbolWriter \
	System.Core		\
	System			\
	System.XML		\
	System.Net		\
	System.Xml.Linq	\
	System.Xml.Serialization	\
	System.Runtime.Serialization	\
	System.ServiceModel	\
	System.ServiceModel.Web

monotouch_dirs := \
	corlib	\
	Mono.CompilerServices.SymbolWriter	\
	System.Core	\
	System	\
	System.XML	\
	Mono.Security	\
	System	\
	I18N		\
	System.Xml.Linq	\
	System.Runtime.Serialization	\
	System.ServiceModel	\
	System.ServiceModel.Web	\
	System.Json	\
	System.Web.Services	\
	Mono.Data.Tds	\
	System.Transactions	\
	System.Data	\
	Mono.Cairo	\
	Mono.Data.Sqlite

net_3_5_dirs := \
	Microsoft.Build.Framework	\
	Microsoft.Build.Utilities	\
	Microsoft.Build.Engine		\
	Microsoft.Build.Tasks		\
	Moonlight.Build.Tasks		\
	System.Web.Extensions.Design	\
	System.Data.Services.Client

net_4_0_dirs := \
	System.Dynamic		\
	System.Numerics		\
	Microsoft.CSharp	\
	System.Windows.Forms.DataVisualization	\
	System.ComponentModel.Composition \
	System.Xaml \
	System.ServiceModel.Routing \
	System.ServiceModel.Discovery \
	System.Runtime.Caching \
	System.Runtime.DurableInstancing \
	Mono.CodeContracts

net_2_0_bootstrap_SUBDIRS := $(bootstrap_dirs)
net_2_0_SUBDIRS := $(common_dirs) $(net_2_0_dirs) $(net_2_0_only_dirs)
moonlight_bootstrap_SUBDIRS := corlib System Mono.CompilerServices.SymbolWriter System.Core
moonlight_raw_SUBDIRS := $(moonlight_dirs)
monotouch_bootstrap_SUBDIRS := corlib System Mono.CompilerServices.SymbolWriter System.Core
monotouch_SUBDIRS := $(monotouch_dirs)
net_3_5_SUBDIRS := $(net_3_5_dirs)
net_4_0_bootstrap_SUBDIRS := $(bootstrap_dirs) Mono.Posix System.Core
net_4_0_SUBDIRS := $(common_dirs) $(net_2_0_dirs) $(net_4_0_dirs)

include ../build/rules.make

SUBDIRS = $(common_dirs) $(net_2_0_dirs) $(net_2_0_only_dirs) $(moonlight_dirs) $(monotouch_dirs) $(net_3_5_dirs) $(net_4_0_dirs)

DIST_ONLY_SUBDIRS = dlr

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

basic_files = gmcs.exe mscorlib.dll System.dll System.Xml.dll Mono.Security.dll System.Core.dll
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

