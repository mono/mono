TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config

ALL_TARGETS = harness.exe appbuilder.exe

ifdef ENABLE_NETCORE
ALL_TARGETS += netcore-test-runner.exe
endif

ifndef DISABLE_CLASSIC
ALL_TARGETS += test-runner.exe
endif

all: $(ALL_TARGETS)

BCL_DIR = ../out/ios-bcl/monotouch
NETCORE_BCL_DIR = ../out/ios-netcore_libs/ios
XCODE_ROOT=$(shell xcode-select -p)
SYSROOT=$(XCODE_ROOT)/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk

NINJA_PATH:=$(or $(shell which ninja),$(error "ninja not found, install it using 'brew install ninja'."))
IOS_DEPLOY_PATH:=$(or $(shell which ios-deploy),$(error "ios-deploy not found, install it using 'brew install ios-deploy'."))

# Default to add-hoc signing
IOS_SIGNING_IDENTITY ?= -

TEST_SUITES = \
	Mono.Runtime.Tests \
	corlib \
	System.Core \
	System.Data \
	System.Numerics \
	System.Runtime.Serialization \
	System.Transactions \
	System.IO.Compression \
	System.IO.Compression.FileSystem \
	System.Json \
	System.ComponentModel.DataAnnotations \
	System.Security \
	System.Xml \
	System.Xml.Linq \
	System.ServiceModel.Web \
	Mono.Data.Tds \
	Mono.Security

# Compiling but has failures
TEST_SUITES_COMPILING = \
	System \
	System.Net.Http \
	Mono.CSharp

# Not compiling in monotouch profile
TEST_SUITES_NOTCOMPILING = \
	System.Web.Services \
	Mono.Data.Sqlite

SIM_NAME = xamarin.ios-sdk.sim

OPTIONS_CS = ../../mcs/class/Mono.Options//Mono.Options/Options.cs

harness.exe: harness/harness.cs $(OPTIONS_CS)
	csc /out:$@ -r:System.Json.dll $^

appbuilder.exe: appbuilder/appbuilder.cs $(OPTIONS_CS)
	csc /out:$@ $^

NETCORE_XUNIT_DIR := $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/System.Buffers.Tests/netcoreapp-Debug

netcore-test-runner.exe: test-runner/runner.cs $(NETCORE_XUNIT_DIR)/xunit.core.dll
	csc /out:$@ /debug /noconfig /nostdlib /d:ENABLE_NETCORE /d:XUNIT_RUNNER /r:$(NETCORE_BCL_DIR)/System.Private.CoreLib.dll /r:$(NETCORE_BCL_DIR)/System.Runtime.dll /r:$(NETCORE_BCL_DIR)/System.Runtime.Extensions.dll /r:$(NETCORE_BCL_DIR)/System.Xml.XDocument.dll /r:$(NETCORE_BCL_DIR)/System.Private.Xml.dll /r:$(NETCORE_BCL_DIR)/System.Private.Xml.Linq.dll  /r:$(NETCORE_BCL_DIR)/System.Linq.dll /r:$(NETCORE_BCL_DIR)/System.IO.dll /r:$(NETCORE_BCL_DIR)/System.IO.FileSystem.dll /r:$(NETCORE_BCL_DIR)/System.Collections.dll /r:$(NETCORE_BCL_DIR)/System.Threading.dll /r:$(NETCORE_BCL_DIR)/System.Net.Sockets.dll /r:$(NETCORE_XUNIT_DIR)/xunit.core.dll /r:$(NETCORE_XUNIT_DIR)/xunit.abstractions.dll /r:$(NETCORE_XUNIT_DIR)/xunit.runner.utility.netcoreapp10.dll test-runner/runner.cs

test-runner.exe: test-runner/runner.cs
	csc /out:$@ /debug -r:$(BCL_DIR)/nunitlite.dll -r:$(BCL_DIR)/Mono.Security.dll test-runner/runner.cs

app/app-simulator app/app-device.a app/app-netcore-simulator app/app-netcore-device.a: app/*.h app/*.m app/Makefile
	$(MAKE) -C app

ifdef ENABLE_NETCORE
SIM_DEPS += netcore-test-runner.exe app/app-netcore-simulator
endif

ifndef DISABLE_CLASSIC
SIM_DEPS += test-runner.exe app/app-simulator
endif

# Build % from assemblies %_ASSEMBLIES
# The end result is in bin/ios-sim/test-%.app

build-ios-sim-%: appbuilder.exe $(SIM_DEPS)
	mono appbuilder.exe $(APPBUILDER_ARGS) $($*_APPBUILDER_ARGS) --target ios-sim64 --mono-sdkdir $(abspath ../out) --appdir $(abspath bin/ios-sim/test-$*.app) --apptemplatedir $(abspath app) --builddir $(abspath obj/ios-sim/test-$*.app) --sysroot $(SYSROOT) --signing-identity - --bundle-executable test-$* --bundle-identifier com.xamarin.mono.ios.test-$* --bundle-name test-$* $(patsubst %,-r %,$($*_ASSEMBLIES))
	mkdir -p bin/ios-sim/test-$*.app
	ninja -C obj/ios-sim/test-$*.app -v

ifdef LLVM
APPBUILDER_ARGS += --llvm
endif

ifdef INTERP_ONLY
APPBUILDER_ARGS += --interp-only
endif

ifdef INTERP_MIXED
APPBUILDER_ARGS += --interp-mixed
endif

#
# This enables caching of aot outputs between different apps.
# Changes to the assemblies/runtimes etc. are not detected, so this should only
# be used when compiling multiple apps with the same assemblies, i.e. the -all
# targets.
#
ifdef ENABLE_AOT_CACHE
APPBUILDER_ARGS += --aot-cachedir $(abspath aot-cache)
endif

ifdef ENABLE_NETCORE
DEV_DEPS += netcore-test-runner.exe app/app-netcore-device.a
endif

ifndef DISABLE_CLASSIC
DEV_DEPS += test-runner.exe app/app-device.a
endif

build-ios-dev-%: appbuilder.exe $(DEV_DEPS)
ifeq ($(IOS_SIGNING_IDENTITY),-)
	@echo "Error: Ad-hoc signing isn't valid for device builds, set IOS_SIGNING_IDENTITY." && exit 1
endif
ifeq ($(IOS_TEAM_IDENTIFIER),)
	@echo "Error: Team identifier needs to be set for device builds, set IOS_TEAM_IDENTIFIER." && exit 1
endif
	mkdir -p aot-cache
	mono appbuilder.exe $(APPBUILDER_ARGS) $($*_APPBUILDER_ARGS) --target ios-dev64 --mono-sdkdir $(abspath ../out) --appdir $(abspath bin/ios-dev/test-$*.app) --apptemplatedir $(abspath app) --builddir $(abspath obj/ios-dev/test-$*.app) --sysroot $(SYSROOT) --signing-identity "$(IOS_SIGNING_IDENTITY)" --team-identifier "$(IOS_TEAM_IDENTIFIER)" --bundle-executable test-$* --bundle-identifier com.xamarin.mono.ios.test-$* --bundle-name test-$* $(patsubst %,-r %,$($*_ASSEMBLIES))
	mkdir -p bin/ios-dev/test-$*.app
	ninja -C obj/ios-dev/test-$*.app -v

# Clean %
clean-ios-sim-%:
	$(RM) -rf obj/ios-sim/test-$*.app bin/ios-sim/test-$*.app

clean-ios-dev-%:
	$(RM) -rf obj/ios-dev/test-$*.app bin/ios-dev/test-$*.app

ifdef DEBUG
APP_ARGS += --wait-for-debugger
APPBUILDER_ARGS += --debug
endif

# Install and run % on the simulator with args $(APP_ARGS) %_ARGS
run-ios-sim-%: harness.exe
	mono harness.exe --start-sim
	xcrun simctl install $(SIM_NAME) bin/ios-sim/test-$*.app
	mono harness.exe --run-sim --logfile ios-sim-$*.log --bundle-id com.xamarin.mono.ios.test-$* --bundle-dir bin/ios-sim/test-$*.app $(APP_ARGS) $($*_ARGS)

run-ios-dev-%: harness.exe
	mono harness.exe --run-dev --logfile ios-dev-$*.log --bundle-id com.xamarin.mono.ios.test-$* --bundle-dir bin/ios-dev/test-$*.app $(APP_ARGS) $($*_ARGS)

clean:
	$(MAKE) -C app clean
	$(RM) -rf bin obj *.exe *.log aot-cache

build-ios-sim-all:
	for suite in $(TEST_SUITES); do $(MAKE) build-ios-sim-$$suite || exit 1; done

run-ios-sim-all:
	for suite in $(TEST_SUITES); do $(MAKE) run-ios-sim-$$suite || exit 1; done

build-ios-dev-all:
	rm -rf aot-cache
	for suite in $(TEST_SUITES); do $(MAKE) clean-ios-dev-$$suite; $(MAKE) build-ios-dev-$$suite ENABLE_AOT_CACHE=1 || exit 1; done

build-ios-dev-llvm-all:
	rm -rf aot-cache
	for suite in $(TEST_SUITES); do echo "*** $$suite ***"; $(MAKE) clean-ios-dev-$$suite; $(MAKE) build-ios-dev-$$suite LLVM=1 ENABLE_AOT_CACHE=1 || exit 1; done

build-ios-dev-interp-only-all:
	rm -rf aot-cache
	for suite in $(TEST_SUITES); do echo "*** $$suite ***"; $(MAKE) clean-ios-dev-$$suite; $(MAKE) build-ios-dev-$$suite INTERP_ONLY=1 ENABLE_AOT_CACHE=1 || exit 1; done

build-ios-dev-interp-mixed-all:
	rm -rf aot-cache
	for suite in $(TEST_SUITES); do echo "*** $$suite ***"; $(MAKE) clean-ios-dev-$$suite; $(MAKE) build-ios-dev-$$suite INTERP_MIXED=1 ENABLE_AOT_CACHE=1 || exit 1; done

run-ios-dev-all:
	for suite in $(TEST_SUITES); do $(MAKE) run-ios-dev-$$suite || exit 1; done

# Developer targets, ignore
# 'launch' doesn't propagate the error code
# With ios11, --console doesn't work any more, it makes the app deadlock
# Install % on the simulator
install-ios-sim-%:
	xcrun simctl install $(SIM_NAME) bin/ios-sim/test-$*.app

run-ios-sim-direct-%:
	xcrun simctl terminate $(SIM_NAME) com.xamarin.mono.ios.test-$*
	xcrun simctl launch $(SIM_NAME) com.xamarin.mono.ios.test-$* $(APP_ARGS) $($*_ARGS)
	log stream --level debug --predicate 'senderImagePath contains "$*"' --style compact

create-sim:
	xcrun simctl create $(SIM_NAME) 'iPhone 7' com.apple.CoreSimulator.SimRuntime.iOS-11-1

start-sim:
	xcrun simctl boot $(SIM_NAME)

stop-sim:
	xcrun simctl shutdown $(SIM_NAME)

ifdef ENABLE_NETCORE

NETCORE_TEST_ASSEMBLIES = $(filter-out $(NETCORE_BCL_DIR)/System.Runtime.WindowsRuntime.dll $(NETCORE_BCL_DIR)/System.Runtime.WindowsRuntime.UI.Xaml.dll, $(wildcard $(NETCORE_BCL_DIR)/*.dll)) netcore-test-runner.exe

# Parameters:
#  $(1): test assembly name (no .dll suffix)
define NetCoreTestTemplate

$(1)_ASSEMBLIES = \
	$(NETCORE_TEST_ASSEMBLIES) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/$(1).dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/CoreFx.Private.TestUtilities.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Newtonsoft.Json.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/xunit.abstractions.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/xunit.assert.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/xunit.core.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/xunit.execution.dotnet.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/xunit.runner.utility.netcoreapp10.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.VisualStudio.TestPlatform.ObjectModel.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.DotNet.PlatformAbstractions.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.DotNet.XUnitExtensions.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.TestPlatform.CommunicationUtilities.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.TestPlatform.CoreUtilities.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.TestPlatform.CrossPlatEngine.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.TestPlatform.PlatformAbstractions.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.TestPlatform.Utilities.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.VisualStudio.CodeCoverage.Shim.dll) \
	$(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/$(1)/netcoreapp*/Microsoft.VisualStudio.TestPlatform.Common.dll)

# TODO: remove need for DOTNET_SYSTEM_GLOBALIZATION_INVARIANT by compiling System.Globalization.Native.dylib
# TODO: replace nonosxtests trait with noniostests
$(1)_ARGS = --setenv=DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 netcore-test-runner.exe CONNSTR $(1).dll -notrait category=nonosxtests @CoreFX.issues.rsp @CoreFX.issues_mac.rsp
$(1)_APPBUILDER_ARGS += --netcore --exe netcore-test-runner.exe --content="$(abspath $(TOP)/netcore/CoreFX.issues.rsp)" --content="$(abspath $(TOP)/netcore/CoreFX.issues_mac.rsp)"

endef

$(foreach testdir,$(notdir $(wildcard $(DOTNET_RUNTIME_REPO_DIR)/artifacts/bin/*.Tests)),$(eval $(call NetCoreTestTemplate,$(testdir))))

endif

ifndef DISABLE_CLASSIC

CLASSIC_TEST_ASSEMBLIES = $(BCL_DIR)/mscorlib.dll \
	$(BCL_DIR)/System.dll \
	$(BCL_DIR)/System.Xml.dll \
	$(BCL_DIR)/System.Core.dll \
	$(BCL_DIR)/I18N.dll \
	$(BCL_DIR)/I18N.West.dll \
	$(BCL_DIR)/Mono.Simd.dll \
	$(BCL_DIR)/Mono.Security.dll \
	$(BCL_DIR)/System.Numerics.dll \
	$(BCL_DIR)/System.Numerics.Vectors.dll \
	$(BCL_DIR)/nunitlite.dll \
	test-runner.exe

# CONNSTR will be replace by the harness with the real connection string
NUNIT = test-runner.exe CONNSTR -exclude:MobileNotWorking,NotOnMac,NotWorking,CAS,InetAccess,NotWorkingLinqInterpreter $(if $(TESTNAME),-test:$(TESTNAME)) -labels
TESTDIR = $(BCL_DIR)/tests

# Options for each test
Mono.Runtime.Tests_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(TESTDIR)/monotouch_Mono.Runtime.Tests_test.dll
Mono.Runtime.Tests_ARGS = $(NUNIT) monotouch_Mono.Runtime.Tests_test.dll
corlib_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(TESTDIR)/monotouch_corlib_test.dll
corlib_ARGS = $(NUNIT) monotouch_corlib_test.dll
System.Core_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(TESTDIR)/monotouch_System.Core_test.dll
System.Core_ARGS = $(NUNIT) monotouch_System.Core_test.dll
System_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(TESTDIR)/monotouch_System_test.dll $(BCL_DIR)/Mono.Security.dll
System_ARGS = $(NUNIT) monotouch_System_test.dll
System.Data_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Data.dll $(BCL_DIR)/System.Transactions.dll $(TESTDIR)/monotouch_System.Data_test.dll
System.Data_ARGS = $(NUNIT) monotouch_System.Data_test.dll
System.Net.Http_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Net.Http.dll $(TESTDIR)/monotouch_System.Net.Http_test.dll
System.Net.Http_ARGS = $(NUNIT) monotouch_System.Net.Http_test.dll
System.Numerics_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(TESTDIR)/monotouch_System.Numerics_test.dll
System.Numerics_ARGS = $(NUNIT) monotouch_System.Numerics_test.dll
System.Runtime.Serialization_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Runtime.Serialization.dll $(BCL_DIR)/System.ServiceModel.dll $(BCL_DIR)/System.ServiceModel.Internals.dll $(TESTDIR)/monotouch_System.Runtime.Serialization_test.dll
System.Runtime.Serialization_ARGS = $(NUNIT) monotouch_System.Runtime.Serialization_test.dll
System.Transactions_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Transactions.dll $(TESTDIR)/monotouch_System.Transactions_test.dll
System.Transactions_ARGS = $(NUNIT) monotouch_System.Transactions_test.dll
System.IO.Compression_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.IO.Compression.dll $(TESTDIR)/monotouch_System.IO.Compression_test.dll
System.IO.Compression_ARGS = $(NUNIT) monotouch_System.IO.Compression_test.dll
System.IO.Compression.FileSystem_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.IO.Compression.FileSystem.dll $(BCL_DIR)/System.IO.Compression.dll $(TESTDIR)/monotouch_System.IO.Compression.FileSystem_test.dll
System.IO.Compression.FileSystem_ARGS = $(NUNIT) monotouch_System.IO.Compression.FileSystem_test.dll
Mono.CSharp_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/Mono.CSharp.dll $(TESTDIR)/monotouch_Mono.CSharp_test.dll
Mono.CSharp_ARGS = $(NUNIT) monotouch_Mono.CSharp_test.dll
System.Json_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Json.dll $(TESTDIR)/monotouch_System.Json_test.dll
System.Json_ARGS = $(NUNIT) monotouch_System.Json_test.dll
System.ComponentModel.DataAnnotations_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.ComponentModel.DataAnnotations.dll $(TESTDIR)/monotouch_System.ComponentModel.DataAnnotations_test.dll
System.ComponentModel.DataAnnotations_ARGS = $(NUNIT) monotouch_System.ComponentModel.DataAnnotations_test.dll
Mono.Data.Sqlite_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/Mono.Data.Sqlite.dll $(TESTDIR)/monotouch_Mono.Data.Sqlite_test.dll
Mono.Data.Sqlite_ARGS = $(NUNIT) monotouch_Mono.Data.Sqlite_test.dll
Mono.Data.Tds_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/Mono.Data.Tds.dll $(TESTDIR)/monotouch_Mono.Data.Tds_test.dll
Mono.Data.Tds_ARGS = $(NUNIT) monotouch_Mono.Data.Tds_test.dll
System.Security_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Security.dll $(TESTDIR)/monotouch_System.Security_test.dll
System.Security_ARGS = $(NUNIT) monotouch_System.Security_test.dll
System.Xml_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Data.dll $(BCL_DIR)/System.Transactions.dll $(TESTDIR)/monotouch_System.Xml_test.dll
System.Xml_ARGS = $(NUNIT) monotouch_System.Xml_test.dll
System.Xml.Linq_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Xml.Linq.dll $(TESTDIR)/monotouch_System.Xml.Linq_test.dll
System.Xml.Linq_ARGS = $(NUNIT) monotouch_System.Xml.Linq_test.dll
Mono.Security_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(TESTDIR)/monotouch_Mono.Security_test.dll
Mono.Security_ARGS = $(NUNIT) monotouch_Mono.Security_test.dll
System.ServiceModel.Web_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.ServiceModel.Web.dll $(BCL_DIR)/System.ServiceModel.dll $(BCL_DIR)/System.ServiceModel.Internals.dll $(BCL_DIR)/System.IdentityModel.dll $(BCL_DIR)/System.Runtime.Serialization.dll $(TESTDIR)/monotouch_System.ServiceModel.Web_test.dll
System.ServiceModel.Web_ARGS = $(NUNIT) monotouch_System.ServiceModel.Web_test.dll
System.Web.Services_ASSEMBLIES = $(CLASSIC_TEST_ASSEMBLIES) $(BCL_DIR)/System.Web.Services.dll $(TESTDIR)/monotouch_System.Web.Services_test.dll
System.Web.Services_ARGS = $(NUNIT) monotouch_System.Web.Services_test.dll

endif
