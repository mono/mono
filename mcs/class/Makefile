thisdir = class

basic_SUBDIRS := corlib Mono.Security System System.XML System.Core 

# resgen is corlib specific so we need to wait until corlib is build
# and build it just a step before first dll needs it
resgen_dir = ../tools/resgen

build_SUBDIRS :=  \
	corlib \
	Mono.Security \
	System \
	System.XML \
	Mono.Posix \
	System.Core \
	Mono.Cecil \
	Mono.Cecil.Mdb

pcl_facade_dirs := Facades

mobile_common_dirs := \
	corlib	\
	Mono.Security \
	System	\
	System.Core	\
	System.Security	\
	System.XML	\
	I18N		\
	System.ServiceModel.Internals	\
	System.Runtime.Serialization	\
	System.Xml.Linq	\
	System.IdentityModel		\
	System.ServiceModel	\
	System.ServiceModel.Web	\
	System.Json	\
	System.Web.Services	\
	Mono.Data.Tds	\
	System.Transactions	\
	System.Numerics	\
	System.Numerics.Vectors		\
	System.Data	\
	Mono.Data.Sqlite	\
	System.Data.Services.Client \
	System.IO.Compression \
	System.IO.Compression.FileSystem \
	System.ComponentModel.DataAnnotations \
	System.ComponentModel.Composition.4.5 \
	System.Net \
	System.Net.Http	\
	System.Windows \
	System.Xml.Serialization \
	Mono.CSharp	\
	Microsoft.CSharp \
	Mono.Security.Providers.DotNet	\
	Mono.Security.Providers.NewSystemSource \
	Mono.Security.Providers.NewTls	\
	System.Runtime.InteropServices.RuntimeInformation	\
	System.Reflection.DispatchProxy	\
	System.Xml.XPath.XmlDocument \
	System.Reflection.Context	\
	System.Net.Http.WinHttpHandler

mobile_static_dirs := \
	$(mobile_common_dirs)	\
	Mono.Dynamic.Interpreter	\
	PEAPI	\
	Mono.CompilerServices.SymbolWriter	\
	Mono.Simd			\
	$(pcl_facade_dirs)

mobile_dynamic_dirs := \
	$(mobile_common_dirs)	\
	Mono.CompilerServices.SymbolWriter	\
	$(pcl_facade_dirs)

xammac_dirs := \
	$(mobile_dynamic_dirs)

monodroid_dirs := \
	$(mobile_dynamic_dirs)

monotouch_dirs := \
	$(mobile_static_dirs)

monotouch_watch_dirs := $(monotouch_dirs)
monotouch_tv_dirs   := $(monotouch_dirs)

monotouch_runtime_dirs := \
	corlib \
	Mono.Security \
	System \
	System.Core \
	System.XML \
	Mono.CSharp

monotouch_watch_runtime_dirs := $(monotouch_runtime_dirs)
monotouch_tv_runtime_dirs := $(monotouch_runtime_dirs)

xammac_4_5_dirs := \
	corlib	\
	Mono.Security \
	System	\
	Mono.Posix			\
	System.Core	\
	System.Security			\
	System.XML	\
	I18N		\
	System.ServiceModel.Internals	\
	SMDiagnostics	\
	System.Numerics	\
	Mono.Data.Tds	\
	System.Transactions	\
	System.EnterpriseServices	\
	System.Data	\
	System.Runtime.Serialization	\
	System.Xml.Linq	\
	Mono.Data.Sqlite	\
	System.Runtime.Serialization.Formatters.Soap \
	System.ComponentModel.DataAnnotations \
	System.IdentityModel		\
	System.IdentityModel.Selectors	\
	Mono.Messaging			\
	System.Configuration.Install 	\
	System.Messaging	        \
	System.Web.Services \
	System.ServiceModel	\
	System.ServiceModel.Web	\
	System.Json	\
	System.Data.Services.Client \
	System.IO.Compression \
	System.IO.Compression.FileSystem \
	System.ComponentModel.Composition.4.5 \
	System.Net \
	System.Windows \
	System.Xml.Serialization \
	Mono.CSharp	\
	Microsoft.CSharp \
	Mono.CompilerServices.SymbolWriter	\
	System.Data.Linq		\
	System.Net.Http \
	System.Net.Http.WebRequest \
	Mono.Security.Providers.DotNet \
	Mono.Security.Providers.OldTls \
	Mono.Security.Providers.NewSystemSource \
	Mono.Security.Providers.NewTls \
	System.Runtime.InteropServices.RuntimeInformation \
	System.Reflection.Context	\
	System.Net.Http.WinHttpHandler	\
	$(pcl_facade_dirs)

net_4_x_dirs := \
	corlib				\
	Mono.Security \
	System				\
	System.XML			\
	Mono.CompilerServices.SymbolWriter \
	Mono.Posix			\
	System.Core			\
	System.Security			\
	System.Configuration	\
	$(resgen_dir)			\
	System.IO.Compression 		\
	System.IO.Compression.FileSystem \
	System.Drawing			\
	System.Transactions		\
	System.EnterpriseServices	\
	Mono.Data.Tds			\
	System.Numerics		\
	System.Numerics.Vectors		\
	System.Data			\
	System.ComponentModel.DataAnnotations	\
	Accessibility			\
	Mono.WebBrowser			\
	System.Runtime.Serialization.Formatters.Soap \
	System.Windows.Forms		\
	Mono.Data.Sqlite		\
	System.Web.ApplicationServices	\
	Novell.Directory.Ldap		\
	System.DirectoryServices	\
	System.Web			\
	System.Web.Services		\
	System.Design			\
	System.Runtime.Remoting		\
	System.Configuration.Install 	\
	System.Management		\
	System.Data.OracleClient	\
	Cscompmgd			\
	Commons.Xml.Relaxng		\
	Mono.Messaging			\
	System.Messaging	        \
	System.ServiceProcess           \
	System.Drawing.Design		\
	ICSharpCode.SharpZipLib		\
	IBM.Data.DB2			\
	CustomMarshalers		\
	SystemWebTestShim			\
	System.ServiceModel.Internals	\
	SMDiagnostics	\
	System.Runtime.Serialization	\
	System.Xml.Linq			\
	System.Data.Linq		\
	System.Web.Abstractions		\
	System.Web.Routing		\
	System.Runtime.DurableInstancing \
	System.IdentityModel		\
	System.IdentityModel.Selectors	\
	System.ServiceModel		\
	System.Web.Extensions		\
	System.Web.Extensions.Design	\
	System.Web.DynamicData		\
	Mono.CSharp			\
	System.Net			\
	System.Json		\
	Microsoft.CSharp	\
	System.Xaml \
	WindowsBase \
	System.ServiceModel.Activation \
	System.ServiceModel.Routing \
	System.ServiceModel.Discovery \
	System.Runtime.Caching \
	Microsoft.Web.Infrastructure \
	System.ServiceModel.Web \
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
	Mono.Security.Providers.NewSystemSource \
	Mono.Security.Providers.NewTls \
	System.Runtime.InteropServices.RuntimeInformation

# These are the subdirs which depends on libs in net_4_x_dirs
# or have proper dependencies between each other
net_4_x_parallel_dirs := \
	PEAPI				\
	I18N				\
	Mono.Http			\
	Mono.Cairo                      \
	Mono.Cecil			\
	Mono.Cecil.Mdb			\
	Mono.Debugger.Soft		\
	Mono.C5				\
	Mono.Management			\
	Mono.Options 			\
	Mono.Simd			\
	Mono.Tasklets       		\
	Mono.CodeContracts \
	Mono.Parallel \
	Mono.Security.Win32		\
	Mono.Messaging.RabbitMQ		\
	System.Dynamic			\
	System.Windows.Forms.DataVisualization	\
	System.Reactive.Interfaces \
	System.Reactive.Core \
	System.Reactive.Linq \
	System.Reactive.PlatformServices \
	System.Reactive.Providers \
	System.Reactive.Runtime.Remoting \
	System.Reactive.Windows.Forms \
	System.Reactive.Windows.Threading \
	System.Reactive.Observable.Aliases \
	System.Reactive.Experimental \
	System.Reactive.Debugger \
	System.Data.Services.Client	\
	System.Data.Services \
	System.Data.Entity \
	System.Data.DataSetExtensions	\
	System.Json.Microsoft \
	System.Threading.Tasks.Dataflow \
	System.ComponentModel.Composition.4.5 \
	System.Windows \
	System.Xml.Serialization \
	Mono.Security.Providers.DotNet \
	Mono.Security.Providers.OldTls \
	System.DirectoryServices.Protocols	\
	RabbitMQ.Client			\
	Microsoft.VisualC		\
	WebMatrix.Data \
	monodoc	\
	System.Deployment \
	System.Web.Mobile \
	System.Web.RegularExpressions \
	System.Workflow.Activities \
	System.Workflow.ComponentModel \
	System.Workflow.Runtime \
	System.Reflection.Context \
	legacy/Mono.Cecil \
	$(pcl_facade_dirs)

xbuild_2_0_dirs := \
	Microsoft.Build.Framework	\
	Microsoft.Build.Utilities	\
	Microsoft.Build.Engine		\
	Mono.XBuild.Tasks		\
	Microsoft.Build.Tasks

xbuild_4_0_dirs := \
	$(xbuild_2_0_dirs)	\
	Microsoft.Build

monodroid_SUBDIRS := $(monodroid_dirs)
monotouch_SUBDIRS := $(monotouch_dirs)
monotouch_watch_SUBDIRS := $(monotouch_watch_dirs)
monotouch_tv_SUBDIRS    := $(monotouch_tv_dirs)
monotouch_runtime_SUBDIRS := $(monotouch_runtime_dirs)
monotouch_watch_runtime_SUBDIRS := $(monotouch_watch_runtime_dirs)
monotouch_tv_runtime_SUBDIRS := $(monotouch_tv_runtime_dirs)
mobile_static_SUBDIRS := $(mobile_static_dirs)
mobile_SUBDIRS := $(mobile_dynamic_dirs)
xammac_SUBDIRS := $(xammac_dirs)
xammac_net_4_5_SUBDIRS := $(xammac_4_5_dirs)
binary_reference_assemblies_SUBDIRS := reference-assemblies
net_4_x_SUBDIRS := $(net_4_x_dirs) $(xbuild_4_0_dirs)
net_4_x_PARALLEL_SUBDIRS := $(net_4_x_parallel_dirs) aot-compiler
xbuild_12_SUBDIRS := $(xbuild_4_0_dirs)
xbuild_14_SUBDIRS := $(xbuild_4_0_dirs) Microsoft.NuGet.Build.Tasks

include ../build/rules.make

SUBDIRS = $(mobile_static_dirs) $(mobile_dynamic_dirs) $(monotouch_dirs) $(monodroid_dirs) $(xammac_dirs) $(net_4_x_dirs) $(net_4_x_parallel_dirs) $(xammac_net_4_5_SUBDIRS)

DIST_ONLY_SUBDIRS = dlr aot-compiler reference-assemblies $(xbuild_4_0_dirs) Microsoft.NuGet.Build.Tasks

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
	test-helpers/NetworkHelpers.cs	\
	test-helpers/SocketResponder.cs	\
	$(monolite_files)

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

all-local-aot:

# Files needed to bootstrap C# compiler
basic_files = basic.exe mscorlib.dll System.dll System.Xml.dll Mono.Security.dll System.Core.dll System.Security.dll System.Configuration.dll
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

do-clean:
	$(RM) -rf $(topdir)/class/lib/$(PROFILE)
