thisdir = class

NO_FACADES_PROFILE := $(filter monodroid_tools monotouch_tools wasm_tools binary_reference_assemblies xbuild_12 xbuild_14 netcore, $(PROFILE))

ifndef NO_FACADES_PROFILE
FACADES_FOLDER := Facades
endif

-include ../build/config.make

ifdef MCS_MODE
MCS_MODE_dirs := Mono.Cecil.Mdb
endif

# Tooling dependencies of mscorlib, we build them to /tmp folder for MONO_PATH to work
# reliably for clean and update builds. All deps are built against stable API to run with
# range of boostrap versions and .NET at the same time
build_SUBDIRS = \
	Mono.Cecil \
	$(MCS_MODE_dirs) \
	../tools/cil-stringreplacer \
	PEAPI \
	../ilasm \
	corlib

build_PARALLEL_SUBDIRS = \
	Mono.Security \
	System \
	System.XML \
	System.Security \
	Mono.Posix \
	System.Core \
	System.Configuration \
	System.ComponentModel.Composition.4.5 \
	System.Numerics \
	System.Xml.Linq \
	System.IO.Compression \
	System.IO.Compression.FileSystem \
	System.Net.Http \
	System.Net.Http.WebRequest \
	System.Drawing

ifdef MCS_MODE

build_SUBDIRS += \
	Mono.Security \
	System \
	System.XML \
	System.Security \
	Mono.Posix \
	System.Core \
	Mono.CompilerServices.SymbolWriter

build_PARALLEL_SUBDIRS += ../mcs
endif

mobile_common_dirs := \
	corlib

mobile_common_dirs_parallel := \
	Mono.Security \
	System	\
	System.Core	\
	System.XML	\
	System.Security	\
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
	System.Data.DataSetExtensions \
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
	Microsoft.CSharp \
	System.Reflection.Context	\
	System.Net.Http.WinHttpHandler \
	System.Runtime.CompilerServices.Unsafe

testing_aot_full_dirs_parallel := \
	$(mobile_common_dirs_parallel)	\
	Mono.Simd \
	Mono.CSharp

testing_aot_full_interp_dirs_parallel := $(testing_aot_full_dirs_parallel)

testing_aot_hybrid_dirs_parallel := $(testing_aot_full_dirs_parallel)

xammac_dirs_parallel := \
	$(mobile_common_dirs_parallel)	\
	Mono.CompilerServices.SymbolWriter	\
	Mono.CSharp

monodroid_dirs_parallel := \
	$(mobile_common_dirs_parallel)	\
	Mono.CompilerServices.SymbolWriter	\
	Mono.Btls.Interface	\
	Mono.Posix \
	Mono.CSharp \
	Mono.Debugger.Soft \
	Mono.Runtime.Tests

monodroid_tools_dirs_parallel := \
	Mono.Cecil	\
	Mono.Cecil.Mdb	\
	Mono.CompilerServices.SymbolWriter \
	ICSharpCode.SharpZipLib	\
	monodoc \
	Mono.Debugger.Soft \
	Mono.Profiler.Log

monotouch_tools_dirs_parallel := \
	Mono.Cecil \
	Mono.Cecil.Mdb

monotouch_dirs_parallel := \
	$(mobile_common_dirs_parallel)	\
	Mono.Simd \
	Mono.CSharp \
	Mono.Runtime.Tests

monotouch_watch_dirs_parallel := $(filter-out Mono.Security Mono.Data.Tds,$(monotouch_dirs_parallel))

monotouch_runtime_dirs_parallel := \
	Mono.Security \
	System \
	System.Core \
	System.XML \
	Mono.CSharp

monotouch_tv_dirs_parallel := $(monotouch_dirs_parallel)

monotouch_tv_runtime_dirs_parallel := $(monotouch_runtime_dirs_parallel)

monotouch_watch_runtime_dirs_parallel := \
	System \
	System.Core \
	System.XML \
	Mono.CSharp

monotouch_tv_runtime_dirs := \
	Mono.Security \
	$(monotouch_watch_runtime_dirs_parallel)

winaot_dirs_parallel := \
	$(mobile_common_dirs_parallel)	\
	Mono.Simd

testing_winaot_interp_dirs_parallel := \
	$(mobile_common_dirs_parallel)	\
	Mono.Simd

unreal_dirs_parallel := \
	$(mobile_common_dirs_parallel)

wasm_dirs_parallel := \
	Mono.Security \
	System	\
	System.Core	\
	System.XML	\
	System.Security	\
	I18N		\
	System.Transactions	\
	System.ServiceModel.Internals	\
	System.Runtime.Serialization	\
	System.Data	\
	System.Data.DataSetExtensions	\
	System.Xml.Linq	\
	System.Numerics	\
	System.Numerics.Vectors		\
	System.IO.Compression \
	System.IO.Compression.FileSystem \
	System.ComponentModel.DataAnnotations \
	System.ComponentModel.Composition.4.5 \
	System.Net.Http	\
	Microsoft.CSharp \
	System.Runtime.CompilerServices.Unsafe

wasm_tools_dirs_parallel := \
	Mono.Cecil

orbis_dirs_parallel := \
	$(filter-out Microsoft.CSharp Mono.Data.Sqlite,$(mobile_common_dirs_parallel))

xammac_4_5_dirs_parallel := \
	Mono.Security \
	System	\
	System.XML			\
	System.Security			\
	Mono.Posix	\
	System.Core	\
	System.Configuration	\
	I18N		\
	System.ServiceModel.Internals	\
	SMDiagnostics	\
	System.Numerics	\
	System.Numerics.Vectors		\
	Mono.Data.Tds	\
	System.Transactions	\
	System.EnterpriseServices	\
	System.Data	\
	System.Data.DataSetExtensions	\
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
	System.Reflection.Context	\
	System.Net.Http.WinHttpHandler	\
	System.Runtime.CompilerServices.Unsafe

# Could be just mscorlib but we have unhandled dependency from RabbitMQ.Client.Apigen.exe
net_4_x_dirs := \
	corlib \
	Mono.Security \
	System	\
	System.XML

net_4_x_parallel_dirs = \
	System.Security			\
	Mono.Posix			\
	System.Core			\
	System.Configuration	\
	System.Drawing			\
	System.IO.Compression 		\
	System.IO.Compression.FileSystem \
	System.Transactions		\
	System.EnterpriseServices	\
	System.Numerics		\
	System.Numerics.Vectors		\
	System.Data		\
	System.ComponentModel.DataAnnotations	\
	Accessibility			\
	Mono.WebBrowser			\
	System.Runtime.Serialization.Formatters.Soap \
	System.Windows.Forms		\
	Mono.Data.Sqlite		\
	System.Web.ApplicationServices	\
	Novell.Directory.Ldap		\
	System.DirectoryServices	\
	System.Design			\
	System.Web.Services		\
	System.Web			\
	System.Runtime.Remoting		\
	System.Configuration.Install 	\
	System.Management		\
	System.Data.OracleClient	\
	Commons.Xml.Relaxng		\
	Mono.Messaging			\
	System.Messaging	        \
	System.ServiceProcess           \
	System.Drawing.Design		\
	ICSharpCode.SharpZipLib		\
	Microsoft.CSharp	\
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
	System.Net			\
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
	CustomMarshalers		\
	IBM.Data.DB2			\
	PEAPI				\
	I18N				\
	Cscompmgd			\
	Mono.CompilerServices.SymbolWriter \
	Mono.Btls.Interface		\
	Mono.Http			\
	Mono.Cairo                      \
	Mono.Cecil			\
	Mono.Cecil.Mdb			\
	Mono.CSharp			\
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
	Mono.Data.Tds			\
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
	System.Json		\
	System.Json.Microsoft \
	System.Threading.Tasks.Dataflow \
	System.ComponentModel.Composition.4.5 \
	System.Windows \
	System.Xml.Serialization \
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
	Mono.Profiler.Log	\
	Mono.Runtime.Tests	\
	System.Runtime.CompilerServices.Unsafe \
	legacy/Mono.Cecil \
	SystemWebTestShim \
	$(xbuild_4_0_dirs)

xbuild_4_0_dirs := \
	Microsoft.Build.Framework	\
	Microsoft.Build.Utilities	\
	Microsoft.Build.Engine		\
	Mono.XBuild.Tasks		\
	Microsoft.Build.Tasks	\
	Microsoft.Build

monodroid_SUBDIRS := $(mobile_common_dirs)
monodroid_PARALLEL_SUBDIRS := $(monodroid_dirs_parallel)
monodroid_tools_SUBDIRS := 
monodroid_tools_PARALLEL_SUBDIRS := $(monodroid_tools_dirs_parallel)

monotouch_SUBDIRS := $(mobile_common_dirs)
monotouch_PARALLEL_SUBDIRS := $(monotouch_dirs_parallel)
monotouch_watch_SUBDIRS := $(mobile_common_dirs)
monotouch_watch_PARALLEL_SUBDIRS := $(monotouch_watch_dirs_parallel)
monotouch_tv_SUBDIRS    := $(mobile_common_dirs)
monotouch_tv_PARALLEL_SUBDIRS := $(monotouch_tv_dirs_parallel)
monotouch_runtime_SUBDIRS := $(mobile_common_dirs)
monotouch_runtime_PARALLEL_SUBDIRS := $(monotouch_runtime_dirs_parallel)
monotouch_watch_runtime_SUBDIRS := $(mobile_common_dirs)
monotouch_watch_runtime_PARALLEL_SUBDIRS := $(monotouch_watch_runtime_dirs_parallel)
monotouch_tv_runtime_SUBDIRS := $(mobile_common_dirs)
monotouch_tv_runtime_PARALLEL_SUBDIRS := $(monotouch_tv_runtime_dirs_parallel)
monotouch_tools_SUBDIRS :=
monotouch_tools_PARALLEL_SUBDIRS := $(monotouch_tools_dirs_parallel)

testing_aot_full_interp_SUBDIRS := $(mobile_common_dirs)
testing_aot_full_interp_PARALLEL_SUBDIRS := $(testing_aot_full_interp_dirs_parallel)
testing_aot_full_SUBDIRS := $(mobile_common_dirs)
testing_aot_full_PARALLEL_SUBDIRS := $(testing_aot_full_dirs_parallel)
testing_aot_hybrid_SUBDIRS := $(mobile_common_dirs)
testing_aot_hybrid_PARALLEL_SUBDIRS := $(testing_aot_hybrid_dirs_parallel)

xammac_SUBDIRS := $(mobile_common_dirs)
xammac_PARALLEL_SUBDIRS := $(xammac_dirs_parallel)
xammac_net_4_5_SUBDIRS := $(mobile_common_dirs)
xammac_net_4_5_PARALLEL_SUBDIRS := $(xammac_4_5_dirs_parallel)

binary_reference_assemblies_SUBDIRS := reference-assemblies
net_4_x_SUBDIRS := $(net_4_x_dirs)
net_4_x_PARALLEL_SUBDIRS := $(net_4_x_parallel_dirs)
xbuild_12_SUBDIRS := $(xbuild_4_0_dirs)
xbuild_14_SUBDIRS := $(xbuild_4_0_dirs) Microsoft.NuGet.Build.Tasks

winaot_SUBDIRS := $(mobile_common_dirs)
winaot_PARALLEL_SUBDIRS := $(winaot_dirs_parallel)

testing_winaot_interp_SUBDIRS := $(mobile_common_dirs)
testing_winaot_interp_PARALLEL_SUBDIRS := $(testing_winaot_interp_dirs_parallel)

orbis_SUBDIRS := $(mobile_common_dirs)
orbis_PARALLEL_SUBDIRS := $(orbis_dirs_parallel)

unreal_SUBDIRS := $(mobile_common_dirs)
unreal_PARALLEL_SUBDIRS := $(unreal_dirs_parallel)

wasm_SUBDIRS := $(mobile_common_dirs)
wasm_PARALLEL_SUBDIRS := $(wasm_dirs_parallel)
wasm_tools_SUBDIRS :=
wasm_tools_PARALLEL_SUBDIRS := $(wasm_tools_dirs_parallel)

netcore_SUBDIRS :=
netcore_PARALLEL_SUBDIRS :=

include ../build/rules.make

DIST_SUBDIRS = $(testing_aot_full_dirs_parallel) $(testing_aot_hybrid_dirs_parallel) $(monotouch_dirs_parallel) $(monotouch_tools_dirs_parallel) \
$(monodroid_dirs_parallel) $(monodroid_tools_dirs_parallel) $(xammac_dirs_parallel) $(net_4_x_dirs) $(net_4_x_parallel_dirs) \
$(xammac_4_5_dirs_parallel) $(unreal_dirs_parallel) $(wasm_dirs_parallel) $(wasm_tools_dirs_parallel) $(xbuild_14_SUBDIRS) \
$(testing_aot_full_interp_dirs_parallel) $(netcore_SUBDIRS) dlr aot-compiler reference-assemblies Facades

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
	Open.snk		\
	LICENSE 		\
	README			\
	doc/API-notes		\
	doc/NUnitGuidelines	\
	doc/TemplateTest.cs	\
	notes/BitVecto32.txt	\
	MicrosoftAjaxLibrary/License.htm	\
	test-helpers/AdminHelper.cs \
	test-helpers/Configuration.Http.cs \
	test-helpers/NetworkHelpers.cs \
	test-helpers/NunitHelpers.cs \
	test-helpers/PlatformDetection.cs \
	test-helpers/RemoteExecutorTestBase.Mobile.cs \
	test-helpers/RemoteExecutorTestBase.Mono.cs \
	test-helpers/SocketResponder.cs \
	test-helpers/TestResourceHelper.cs \
	test-helpers/PathHelpers.cs \
	$(foreach HOST_PLATFORM,macos linux win32 unix,lib/$(monolite_dir)/mcs.exe $(monolite_files))

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

all-local-aot:

monolite_dir = monolite-$(HOST_PLATFORM)/$(MONO_CORLIB_VERSION)

# Files needed to bootstrap C# compiler and cil-stringreplacer
build_files := mscorlib.dll System.dll System.Xml.dll Mono.Security.dll System.Core.dll System.Security.dll System.Configuration.dll \
	System.Numerics.dll System.Xml.Linq.dll System.IO.Compression.dll \
	Facades/System.Collections.Concurrent.dll Facades/System.Linq.dll Facades/System.Runtime.dll Facades/System.Collections.dll	\
	Facades/System.Reflection.Extensions.dll Facades/System.Text.Encoding.Extensions.dll Facades/System.Diagnostics.Debug.dll \
	Facades/System.Reflection.Primitives.dll Facades/System.Text.Encoding.dll Facades/System.Diagnostics.Tools.dll Facades/System.Reflection.dll \
	Facades/System.Threading.Tasks.Parallel.dll Facades/System.Dynamic.Runtime.dll Facades/System.Resources.ResourceManager.dll \
	Facades/System.Threading.Tasks.dll Facades/System.Globalization.dll Facades/System.Runtime.Extensions.dll Facades/System.Threading.dll \
	Facades/System.IO.dll Facades/System.Runtime.InteropServices.dll Facades/System.Xml.ReaderWriter.dll Facades/System.Linq.Expressions.dll \
	Facades/System.Runtime.Numerics.dll Facades/System.Xml.XDocument.dll Facades/System.IO.FileSystem.Primitives.dll Facades/System.IO.FileSystem.dll \
	Facades/System.Diagnostics.FileVersionInfo.dll Facades/System.Security.Cryptography.Primitives.dll Facades/System.Security.Cryptography.Algorithms.dll \
	Facades/System.ValueTuple.dll Facades/System.Text.Encoding.CodePages.dll Facades/netstandard.dll \
	Mono.Cecil.dll

monolite_files = $(build_files:%=lib/$(monolite_dir)/%)

lib/$(monolite_dir):
lib/$(monolite_dir)/Facades:
	$(MKINSTALLDIRS) $@

$(monolite_files): | lib/$(monolite_dir)
$(monolite_files): | lib/$(monolite_dir)/Facades
$(monolite_files): lib/$(monolite_dir)/%: lib/build-$(HOST_PLATFORM)/%
	cp -p $< $@

lib/$(monolite_dir)/mcs.exe:
	$(MAKE) -C ../mcs PROFILE=build
	cp -p lib/build-$(HOST_PLATFORM)/mcs.exe lib/$(monolite_dir)

$(build_files:%=lib/build-$(HOST_PLATFORM)/%):
	cd $(topdir) && $(MAKE) profile-do--build--all NO_DIR_CHECK=1 SKIP_AOT=1

dist-monolite: $(monolite_files) lib/$(monolite_dir)/mcs.exe

dist-monolite-all-platforms:
	$(MAKE) dist-monolite HOST_PLATFORM=macos
	$(MAKE) dist-monolite HOST_PLATFORM=linux
	$(MAKE) dist-monolite HOST_PLATFORM=win32
	$(MAKE) dist-monolite HOST_PLATFORM=unix

package-monolite-latest:
	MONOLITE=monolite-$(HOST_PLATFORM)-$(MONO_CORLIB_VERSION)-latest; \
	$(MAKE) dist-monolite monolite_dir=$$MONOLITE; \
	tar zcvpf $$MONOLITE.tar.gz --directory=lib $$MONOLITE/

package-monolite-latest-all-platforms:
	$(MAKE) package-monolite-latest HOST_PLATFORM=macos
	$(MAKE) package-monolite-latest HOST_PLATFORM=linux
	$(MAKE) package-monolite-latest HOST_PLATFORM=win32
	$(MAKE) package-monolite-latest HOST_PLATFORM=unix

monolite_url = https://download.mono-project.com/monolite/monolite-$(BUILD_PLATFORM)-$(MONO_CORLIB_VERSION)-latest.tar.gz

get-monolite-latest:
	-rm -fr lib/monolite-$(BUILD_PLATFORM)/$(MONO_CORLIB_VERSION)
	-mkdir -p lib/monolite-$(BUILD_PLATFORM)
	cd lib && { (wget -O- $(monolite_url) || curl -L $(monolite_url)) | gzip -d | tar xf - ; }
	cd lib && mv -f monolite-$(BUILD_PLATFORM)-$(MONO_CORLIB_VERSION)-latest monolite-$(BUILD_PLATFORM)/$(MONO_CORLIB_VERSION)

dist-default: dist-monolite-all-platforms

dist-local: dist-default

do-clean:
	$(RM) -rf $(topdir)/class/lib/$(PROFILE)

## Aliases for .dep_dirs-* tracker
.PHONY: Microsoft.Build.Utilities.v4.0 System.ComponentModel.Composition System.Xml
Microsoft.Build.Utilities.v4.0: Microsoft.Build.Utilities
System.ComponentModel.Composition: System.ComponentModel.Composition.4.5
System.Xml: System.XML
