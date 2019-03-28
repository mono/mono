
NETCORESDK_VERSION=3.0.100-preview4-010759
NETCORESDK_FILE=dotnet-sdk-$(NETCORESDK_VERSION)-osx-x64.tar.gz
URL:=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$(NETCORESDK_VERSION)/$(NETCORESDK_FILE)

$(NETCORESDK_FILE):
	curl $(URL) --output $(NETCORESDK_FILE)
	tar -xvf $(NETCORESDK_FILE)

build-sample:
	dotnet build sample/HelloWorld

run-sample:
	COMPlus_DebugWriteToStdErr=1 ./dotnet --fx-version "$(VERSION)" sample/HelloWorld/bin/Debug/netcoreapp3.0/HelloWorld.dll

# COREHOST_TRACE=1 
VERSION:=3.0.0-preview4-27514-06
SHAREDRUNTIME:=shared/Microsoft.NETCore.App/$(VERSION)

bcl:
	$(MAKE) -C ../mcs/class/System.Private.CoreLib
	cp ../mcs/class/System.Private.CoreLib/bin/x64/System.Private.CoreLib.dll $(SHAREDRUNTIME)

runtime:
	$(MAKE) -C ../mono
	cp ../mono/mini/.libs/libmonosgen-2.0.dylib $(SHAREDRUNTIME)/libcoreclr.dylib

link-mono:
	cp ../mono/mini/.libs/libmonosgen-2.0.dylib $(SHAREDRUNTIME)/libcoreclr.dylib
	cp ../mcs/class/System.Private.CoreLib/bin/x64/System.Private.CoreLib.{dll,pdb} $(SHAREDRUNTIME)

prepare: $(NETCORESDK_FILE) link-mono

nupkg:
	nuget pack runtime.nuspec -properties VERSION=@VERSION@\;RID=@RID@\;PLATFORM_AOT_SUFFIX=@PLATFORM_AOT_SUFFIX@

COREFX_BINDIR=$(COREFX_ROOT)/artifacts/bin

check-env:
	@if [ "x$(COREFX_ROOT)" == "x" ]; then echo "Set COREFX_ROOT to the root of the fully built corefx repo."; exit 1; fi

#
# Running tests:
#  - Tests are run using dotnet bin/Debug/netcoreapp3.0/<...>.dll
# - One of the test helper assemblies contains pinvokes for System.Globalization.Native, so
#   the dylib has to be in the current working directory when running, since the runtime will
#   try to load the dylib from the same directory as the assembly which refers to it.
#   It has to be symlinked otherwise we load 2 copies of it which fails.
# - RemoteExecutorConsoleApp.exe needs to be in the current directory, since
#   its executed as: 'dotnet RemoteExecutorConsoleApp.exe <test assembly> ...'
# - MONO_PATH needs to be set to the bin dir so the remote executor can find the
#   test assembly.
# - The RemoteExecutorConsoleApp.runtimeconfig.json needs to be changed to use the proper
#   framework version.
#

TEST_SUITES = \
	System.Runtime.Tests \
	System.Runtime.CompilerServices.Unsafe.Tests \
	System.Collections.Tests

# The binary directory under $(COREFIX_BINDIR)
System.Runtime.CompilerServices.Unsafe.Tests_PROFILE = netstandard-Debug
System.Runtime.Tests_PROFILE = netcoreapp-Unix-Debug
System.Runtime.Tests_XUNIT_ARGS = @../excludes-System.Runtime.Tests.rsp
System.Collections.Tests_PROFILE = netcoreapp-Debug
System.Reflection.Emit.Tests_PROFILE = netcoreapp-Debug
System.Reflection.Emit.Tests_XUNIT_ARGS = @../excludes-System.Reflection.Emit.Tests.rsp
System.Reflection.Emit.ILGeneration.Tests_PROFILE = netcoreapp-Debug
System.Reflection.Emit.Lightweight.Tests_PROFILE = netcoreapp-Debug
System.Threading.Tests_PROFILE = netcoreapp-Debug
System.Threading.Thread.Tests_PROFILE = netcoreapp-Debug

build-%: check-env
	cd gen-xunit-runner && dotnet run ../tests $(COREFX_ROOT) $(COREFX_BINDIR)/$*/$($*_PROFILE)/$*.dll -notrait category=nonosxtests -notrait category=failing -notrait category=Outerloop -notrait category=nonnetcoreapptests $($*_XUNIT_ARGS) $(XUNIT_ARGS)
	cp $(COREFX_BINDIR)/RemoteExecutorConsoleApp/netstandard-Debug/RemoteExecutorConsoleApp.* tests/$*
	cp $(COREFX_BINDIR)/System.Runtime.Tests/netcoreapp-Unix-Debug/TestLoadAssembly.dll tests/$*
	ln -sf $(CURDIR)/$(SHAREDRUNTIME)/System.Globalization.Native.dylib tests/$*
	sed -e 's/9.9.9/$(VERSION)/g' < tests/$*/RemoteExecutorConsoleApp.runtimeconfig.json > 2 && mv 2 tests/$*/RemoteExecutorConsoleApp.runtimeconfig.json
	cd tests/$* && dotnet build

run-%: check-env
	cd tests/$* && MONO_PATH=bin/Debug/netcoreapp3.0 MONO_ENV_OPTIONS="--debug --explicit-null-checks" COMPlus_DebugWriteToStdErr=1 ../../dotnet bin/Debug/netcoreapp3.0/$*-runner.dll

all: $(addprefix build-, $(TEST_SUITES))

check: $(addprefix run-, $(TEST_SUITES))

clean:
	rm -rf sdk shared host dotnet tests LICENSE.txt ThirdPartyNotices.txt $(NETCORESDK_FILE)

# e.g. `make xtest-System.Collections.Tests COREFX_ROOT=/prj/corefx`
xtest-%: prepare check-env dl-test-assets
	ln -sf $(CURDIR)/$(SHAREDRUNTIME)/System.Globalization.Native.dylib assets/extracted/$*/
	# we need COREFX_ROOT only for these two files:
	cp $(COREFX_BINDIR)/runtime/netcoreapp-OSX-Debug-x64/xunit.console.deps.json assets/extracted/$*/
	cp $(COREFX_BINDIR)/runtime/netcoreapp-OSX-Debug-x64/CoreFx.Private.TestUtilities.dll assets/extracted/$*/
	cd assets/extracted/$* && \
	(sed -i -e 's/9.9.9/$(VERSION)/g' RemoteExecutorConsoleApp.runtimeconfig.json || true) && \
	COMPlus_DebugWriteToStdErr=1 $(CURDIR)/./dotnet --fx-version "$(VERSION)" xunit.console.dll $*.dll \
		-notrait category=outerloop -notrait category=nonosxtests -notrait category=failing \
		-notrait category=nonnetcoreapptests -noappdomain -noshadow -parallel none -verbose -nunit TestResult-$*-netcore-xunit.xml \
		$(shell grep -v '^#\|^$$' $(CURDIR)/excludes-$*.rsp) $(FIXTURE)

COREFX_TESTS_LIST = \
	Common.Tests \
	CoreFx.Private.TestUtilities.Tests \
	Invariant.Tests \
	#Microsoft.CSharp.Tests \
	Microsoft.VisualBasic.Core.Tests \
	Microsoft.VisualBasic.VB.Tests \
	Microsoft.Win32.Primitives.Tests \
	#Microsoft.XmlSerializer.Generator.Tests \
	System.AppContext.Tests \
	System.Buffers.Tests \
	System.CodeDom.Tests \
	System.Collections.Concurrent.Tests \
	System.Collections.Immutable.Tests \
	System.Collections.NonGeneric.Tests \
	System.Collections.Specialized.Tests \
	System.Collections.Tests \
	System.ComponentModel.Annotations.Tests \
	System.ComponentModel.Composition.Registration.Tests \
	#System.ComponentModel.Composition.Tests \
	System.ComponentModel.EventBasedAsync.Tests \
	System.ComponentModel.Primitives.Tests \
	System.ComponentModel.Tests \
	System.ComponentModel.TypeConverter.Tests \
	System.Composition.AttributeModel.Tests \
	System.Composition.Convention.Tests \
	System.Composition.Hosting.Tests \
	System.Composition.Runtime.Tests \
	System.Composition.Tests \
	System.Composition.TypedParts.Tests \
	System.Configuration.ConfigurationManager.Tests \
	System.Console.Manual.Tests \
	System.Console.Tests \
	#System.Data.Common.Tests \
	System.Data.DataSetExtensions.Tests \
	System.Data.Odbc.Tests \
	System.Data.SqlClient.ManualTesting.Tests \
	System.Data.SqlClient.Stress.Tests \
	System.Data.SqlClient.Tests \
	System.Diagnostics.Contracts.Tests \
	System.Diagnostics.Debug.Tests \
	System.Diagnostics.DiagnosticSource.Tests \
	System.Diagnostics.FileVersionInfo.Tests \
	System.Diagnostics.Process.Tests \
	#System.Diagnostics.StackTrace.Tests \
	System.Diagnostics.TextWriterTraceListener.Tests \
	System.Diagnostics.Tools.Tests \
	System.Diagnostics.TraceSource.Tests \
	#System.Diagnostics.Tracing.Tests \
	System.Drawing.Common.Tests \
	System.Drawing.Primitives.Tests \
	#System.Dynamic.Runtime.Tests \
	System.Globalization.Calendars.Tests \
	System.Globalization.CalendarsWithConfigSwitch.Tests \
	System.Globalization.Extensions.Tests \
	System.Globalization.Tests \
	System.IO.FileSystem.DriveInfo.Tests \
	System.IO.FileSystem.Primitives.Tests \
	System.IO.FileSystem.Tests \
	System.IO.FileSystem.Watcher.Tests \
	System.IO.IsolatedStorage.Tests \
	System.IO.MemoryMappedFiles.Tests \
	System.IO.Packaging.Tests \
	System.IO.Pipelines.Tests \
	System.IO.Pipes.Tests \
	#System.IO.Ports.Tests \
	System.IO.Tests \
	System.IO.UnmanagedMemoryStream.Tests \
	System.Json.Tests \
	#System.Linq.Expressions.Tests \
	System.Linq.Parallel.Tests \
	#System.Linq.Queryable.Tests \
	System.Linq.Tests \
	#System.Memory.Tests \
	#System.Net.Http.Functional.Tests \
	System.Net.Http.Unit.Tests \
	#System.Net.HttpListener.Tests \
	System.Net.Mail.Functional.Tests \
	System.Net.Mail.Unit.Tests \
	#System.Net.NameResolution.Functional.Tests \
	System.Net.NameResolution.Pal.Tests \
	System.Net.NameResolution.Unit.Tests \
	System.Net.NetworkInformation.Functional.Tests \
	System.Net.Ping.Functional.Tests \
	System.Net.Primitives.Functional.Tests \
	System.Net.Primitives.Pal.Tests \
	System.Net.Primitives.UnitTests.Tests \
	System.Net.Requests.Tests \
	System.Net.Security.Tests \
	System.Net.Security.Unit.Tests \
	System.Net.ServicePoint.Tests \
	#System.Net.Sockets.Tests \
	System.Net.WebClient.Tests \
	System.Net.WebHeaderCollection.Tests \
	System.Net.WebProxy.Tests \
	System.Net.WebSockets.Client.Tests \
	System.Net.WebSockets.Tests \
	System.Net.WebSockets.WebSocketProtocol.Tests \
	System.Numerics.Tensors.Tests \
	System.Numerics.Vectors.Tests \
	System.ObjectModel.Tests \
	System.Private.Uri.ExtendedFunctional.Tests \
	System.Private.Uri.Functional.Tests \
	System.Private.Uri.Unit.Tests \
	System.Reflection.Context.Tests \
	System.Reflection.CoreCLR.Tests \
	System.Reflection.DispatchProxy.Tests \
	#System.Reflection.Emit.ILGeneration.Tests \
	System.Reflection.Emit.Lightweight.Tests \
	System.Reflection.Emit.Tests \
	System.Reflection.Extensions.Tests \
	System.Reflection.Metadata.Tests \
	System.Reflection.MetadataLoadContext.Tests \
	#System.Reflection.Tests \
	System.Reflection.TypeExtensions.CoreCLR.Tests \
	System.Reflection.TypeExtensions.Tests \
	System.Resources.Reader.Tests \
	System.Resources.ResourceManager.Tests \
	System.Resources.Writer.Tests \
	System.Runtime.CompilerServices.Unsafe.Tests \
	System.Runtime.CompilerServices.VisualC.Tests \
	System.Runtime.Extensions.Tests \
	System.Runtime.Handles.Tests \
	System.Runtime.InteropServices.RuntimeInformation.Tests \
	#System.Runtime.InteropServices.Tests \
	System.Runtime.InteropServices.WindowsRuntime.Tests \
	System.Runtime.Loader.DefaultContext.Tests \
	System.Runtime.Loader.RefEmitLoadContext.Tests \
	System.Runtime.Loader.Tests \
	System.Runtime.Numerics.Tests \
	#System.Runtime.Serialization.Formatters.Tests \
	System.Runtime.Serialization.Json.ReflectionOnly.Tests \
	#System.Runtime.Serialization.Json.Tests \
	System.Runtime.Serialization.Primitives.Tests \
	System.Runtime.Serialization.Xml.ReflectionOnly.Tests \
	#System.Runtime.Serialization.Xml.Tests \
	System.Runtime.Tests \
	#System.Runtime.Tests \
	System.Security.Claims.Tests \
	System.Security.Cryptography.Algorithms.Tests \
	System.Security.Cryptography.Csp.Tests \
	System.Security.Cryptography.Encoding.Tests \
	System.Security.Cryptography.OpenSsl.Tests \
	System.Security.Cryptography.Pkcs.Tests \
	System.Security.Cryptography.Primitives.Tests \
	System.Security.Cryptography.X509Certificates.Tests \
	System.Security.Cryptography.Xml.Tests \
	System.Security.Permissions.Tests \
	System.Security.SecureString.Tests \
	System.ServiceModel.Syndication.Tests \
	System.Text.Encoding.CodePages.Tests \
	System.Text.Encoding.Extensions.Tests \
	System.Text.Encoding.Tests \
	System.Text.Encodings.Web.Tests \
	System.Text.Json.Tests \
	System.Text.RegularExpressions.Tests \
	System.Threading.Channels.Tests \
	System.Threading.Overlapped.Tests \
	System.Threading.Tasks.Dataflow.Tests \
	System.Threading.Tasks.Extensions.Tests \
	System.Threading.Tasks.Parallel.Tests \
	System.Threading.Tasks.Tests \
	#System.Threading.Tests \
	#System.Threading.Thread.Tests \
	#System.Threading.ThreadPool.Tests \
	#System.Threading.Timer.Tests \
	System.Transactions.Local.Tests \
	System.ValueTuple.Tests \
	System.Web.HttpUtility.Tests \
	System.Xml.Linq.Axes.Tests \
	System.Xml.Linq.Events.Tests \
	System.Xml.Linq.Misc.Tests \
	System.Xml.Linq.Properties.Tests \
	System.Xml.Linq.SDMSample.Tests \
	System.Xml.Linq.Streaming.Tests \
	System.Xml.Linq.TreeManipulation.Tests \
	System.Xml.Linq.xNodeBuilder.Tests \
	System.Xml.Linq.xNodeReader.Tests \
	System.Xml.Misc.Tests \
	System.Xml.RW.CharCheckingReader.Tests \
	System.Xml.RW.CustomReader.Tests \
	System.Xml.RW.FactoryReader.Tests \
	System.Xml.RW.NameTable.Tests \
	System.Xml.RW.ReaderSettings.Tests \
	System.Xml.RW.RwFactory.Tests \
	System.Xml.RW.SubtreeReader.Tests \
	System.Xml.RW.WrappedReader.Tests \
	System.Xml.RW.XmlConvert.Tests \
	System.Xml.RW.XmlReader.ReadContentAs.Tests \
	System.Xml.RW.XmlReader.Tests \
	System.Xml.RW.XmlSystemPathResolver.Tests \
	System.Xml.RW.XmlWriter.Tests \
	System.Xml.RW.XmlWriterApi.Tests \
	System.Xml.Schema.Extensions.Tests \
	System.Xml.XmlDocument.Tests \
	System.Xml.XmlNodeReader.Tests \
	System.Xml.XmlResolver.Tests \
	System.Xml.XmlSchema.XmlSchemaValidatorApi.Tests \
	#System.Xml.XmlSchemaSet.Tests \
	System.Xml.XmlSerializer.ReflectionOnly.Tests \
	#System.Xml.XmlSerializer.Tests \
	System.Xml.XPath.Tests \
	System.Xml.XPath.XDocument.Tests \
	System.Xml.XPath.XmlDocument.Tests \
	#System.Xml.Xsl.XslCompiledTransformApi.Tests \
	System.Xml.Xsl.XslTransformApi.Tests \
	XsltCompiler.Tests

xtest-all: $(addprefix xtest-, $(COREFX_TESTS_LIST))

FEED_BASE_URL = https://dotnetfeed.blob.core.windows.net/dotnet-core
TEST_ASSETS_URL = https://dotnetfeed.blob.core.windows.net/dotnet-core/corefx-tests/4.6.0-preview4.19156.10/OSX.x64/netcoreapp/corefx-test-assets.xml

corefx-test-assets.xml:
	wget $(TEST_ASSETS_URL)

dl-test-assets: corefx-test-assets.xml .stamp-dl-test-assets

.stamp-dl-test-assets:
	python dl-test-assets.py corefx-test-assets.xml $(FEED_BASE_URL) assets
	touch $@
