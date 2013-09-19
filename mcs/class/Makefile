thisdir = class

# Note that Mono.Security and System.Security aren't listed.
# We may have to add those if 'mcs' starts using them.
basic_SUBDIRS := corlib System System.XML System Mono.Security System.Core 

# resgen is corlib specific so we need to wait until corlib is build
# and build it just a step before first dll needs it
resgen_dir = ../tools/resgen

build_SUBDIRS :=  \
	corlib \
	System \
	System.XML \
	System Mono.Security \
	Mono.Posix \
	System.Core

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
	$(resgen_dir)			\
	System.Drawing			\
	System.Transactions		\
	System.EnterpriseServices	\
	Mono.Data.Tds			\
	System.Data			\
	Accessibility			\
	Mono.WebBrowser			\
	System.Runtime.Serialization.Formatters.Soap \
	Managed.Windows.Forms		\
	Mono.Data.Sqlite		\
	System.Web.ApplicationServices	\
	System.Web			\
	Mono.Web			\
	System.Web.Services		\
	System.Web			\
	System.Runtime.Remoting		\
	System.Configuration.Install 	\
	System.Management		\
	System.Data.OracleClient	\
	Microsoft.VisualC		\
	Cscompmgd			\
	System.Design			\
	PEAPI				\
	Npgsql				\
	Commons.Xml.Relaxng		\
	Novell.Directory.Ldap		\
	Mono.Security.Win32		\
	System.DirectoryServices	\
	System.DirectoryServices.Protocols	\
	RabbitMQ.Client			\
	Mono.Messaging			\
	System.Messaging	        \
	Mono.Messaging.RabbitMQ		\
	System.ServiceProcess           \
	System.Drawing.Design		\
	System.Design			\
	ICSharpCode.SharpZipLib		\
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
	System.Web.Mvc2			\
	Mono.C5				\
	Mono.Management			\
	Mono.Options 			\
	Mono.Simd			\
	Mono.Tasklets       		\
	System.Dynamic			\
	Mono.CSharp			\
	Moonlight.Build.Tasks		\
	System.Net				\
	System.Json

net_2_0_only_dirs := \
	System.Web.Extensions_1.0	\
	System.Web.Extensions.Design_1.0	\
	Compat.ICSharpCode.SharpZipLib	\
	WindowsBase			\
	System.ServiceModel.Web \
	System.Data.Services.Client	\
	System.Data.Services		\
	System.Web.Mvc \

mobile_dirs := \
	corlib	\
	System	\
	System.Core	\
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
	Mono.Data.Sqlite	\
	System.Numerics	\
	System.Data.Services.Client \
	System.IO.Compression \
	System.IO.Compression.FileSystem \
	System.ComponentModel.DataAnnotations \
	System.ComponentModel.Composition.4.5 \
	System.Net \
	System.Windows \
	System.Xml.Serialization \
	Facades

monodroid_dirs := \
	Mono.CompilerServices.SymbolWriter	\
	Mono.CSharp	\
	Microsoft.CSharp \
	System.Net.Http

monotouch_runtime_dirs := \
	corlib

net_3_5_only_dirs := \
	Microsoft.Build.Framework	\
	Microsoft.Build.Utilities	\
	Microsoft.Build.Engine		\
	Microsoft.Build.Tasks

net_4_0_dirs := \
	System.Numerics		\
	Microsoft.CSharp	\
	Microsoft.Build		\
	System.Windows.Forms.DataVisualization	\
	System.Xaml \
	WindowsBase \
	System.ServiceModel.Activation \
	System.ServiceModel \
	System.ServiceModel.Routing \
	System.ServiceModel.Discovery \
	System.Runtime.Caching \
	System.Runtime.DurableInstancing \
	Mono.Parallel \
	System.Reactive.Interfaces \
	System.Reactive.Core \
	System.Reactive.Linq \
	System.Reactive.PlatformServices \
	System.Reactive.Providers \
	System.Reactive.Runtime.Remoting \
	System.Reactive.Windows.Forms \
	System.Reactive.Windows.Threading \
	System.Reactive.Experimental \
	System.Reactive.Debugger \
	Microsoft.Web.Infrastructure \
	WebMatrix.Data \
	System.ServiceModel.Web \
	System.Data.Services.Client	\
	System.Data.Services \
	System.Json.Microsoft \
	monodoc
	
net_4_0_only_dirs := \
	System.ComponentModel.Composition

net_4_5_dirs := \
	System.Threading.Tasks.Dataflow \
	System.ComponentModel.Composition.4.5 \
	System.Net.Http	\
	System.Net.Http.WebRequest \
	System.Web.Razor \
	System.Web.WebPages.Deployment \
	System.Web.WebPages \
	System.Web.WebPages.Razor \
	System.Web.Mvc3 \
	System.Net.Http.Formatting \
	System.Web.Http \
	System.Web.Http.SelfHost \
	System.Web.Http.WebHost \
	EntityFramework	\
	EntityFramework.SqlServer	\
	Mono.CodeContracts \
	System.IO.Compression \
	System.IO.Compression.FileSystem \
	System.Windows \
	System.Xml.Serialization \
	Facades
	
net_2_0_SUBDIRS := $(common_dirs) $(net_2_0_dirs) $(net_2_0_only_dirs) aot-compiler
monodroid_SUBDIRS := $(mobile_dirs) $(monodroid_dirs)
monotouch_SUBDIRS := $(mobile_dirs)
monotouch_runtime_SUBDIRS := $(monotouch_runtime_dirs)
mobile_static_SUBDIRS := $(mobile_dirs)
mobile_SUBDIRS := $(mobile_dirs)
net_3_5_SUBDIRS := $(net_3_5_only_dirs)
net_4_0_SUBDIRS := $(common_dirs) $(net_2_0_dirs) $(net_4_0_dirs) $(net_4_0_only_dirs)
net_4_5_SUBDIRS := $(common_dirs) $(net_2_0_dirs) $(net_4_0_dirs) $(net_4_5_dirs) aot-compiler

include ../build/rules.make

SUBDIRS = $(common_dirs) $(net_2_0_dirs) $(net_2_0_only_dirs) $(net_3_5_only_dirs) $(mobile_dirs) $(net_4_0_dirs) $(net_4_0_only_dirs) $(net_4_5_dirs)

DIST_ONLY_SUBDIRS = dlr aot-compiler

# No new makefiles for: System.Messaging, System.Web.Mobile,
# System.ServiceProcess

DISTFILES = \
	ecma.pub		\
	winfx.pub		\
	winfx3.pub		\
	mono.pub		\
	msfinal.pub		\
	silverlight.pub		\
	reactive.pub		\
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

# Files needed to bootstrap C# compiler
basic_files = basic.exe mscorlib.dll System.dll System.Xml.dll Mono.Security.dll System.Core.dll
monolite_files = $(basic_files:%=lib/monolite/%)

lib/monolite:
	$(MKINSTALLDIRS) $@

$(monolite_files): | lib/monolite
$(monolite_files): lib/monolite/%: lib/basic/%
	cp -p $< $@

$(basic_files:%=lib/basic/%):
	cd $(topdir) && $(MAKE) profile-do--basic--all NO_DIR_CHECK=1 SKIP_AOT=1

dist-default: $(monolite_files)
dist-local: dist-default

csproj-local:

