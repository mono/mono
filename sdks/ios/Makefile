
all: harness.exe appbuilder.exe test-runner.exe

BCL_DIR = ../../mcs/class/lib/monotouch

TEST_SUITES = \
	Mono.Runtime.Tests \
	corlib \
	System.Core \
	System.Data \
	System.Net.Http \
	System.Numerics \
	System.Runtime.Serialization \
	System.Transactions \
	System.IO.Compression \
	System.IO.Compression.FileSystem \
	System.Json \
	System.ComponentModel.DataAnnotations \
	System.Security \
	System.Xml.Linq \
	System.ServiceModel.Web \
	Mono.Data.Tds \
	Mono.Security

TEST_SUITES_NOTWORKING = \
	System \
	System.Web.Services \
	System.Xml \
	Mono.Data.Sqlite \
	Mono.CSharp \

SIM_NAME = xamarin.ios-sdk.sim

harness.exe: harness/harness.cs harness/Options.cs
	csc /out:$@ -r:System.Json.dll $^

appbuilder.exe: appbuilder/appbuilder.cs appbuilder/Options.cs
	csc /out:$@ $^

test-runner.exe: test-runner/runner.cs
	csc /out:$@ -r:$(BCL_DIR)/nunitlite.dll test-runner/runner.cs

runtime/runtime:
	make -C runtime

runtime/libmonoios.a:
	make -C runtime

# Build % from assemblies %_ASSEMBLIES
# The end result is in bin/ios-sim/test-%.app

TEST_ASSEMBLIES = $(BCL_DIR)/mscorlib.dll \
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

build-ios-sim-%: appbuilder.exe test-runner.exe runtime/runtime $($*_ASSEMBLIES)
	mono appbuilder.exe --target ios-sim64 --mono-sdkdir $(PWD)/../out --appdir $(PWD)/bin/ios-sim/test-$*.app --runtimedir $(PWD)/runtime --builddir obj/ios-sim/test-$*.app --bundle-executable test-$* --bundle-identifier com.xamarin.mono.ios.test-$* --bundle-name test-$* $(patsubst %,-r %,$(TEST_ASSEMBLIES) $($*_ASSEMBLIES))
	mkdir -p $(PWD)/bin/ios-sim/test-$*.app
	if test "x$($*_EXTRA_FILES)" != "x"; then cp -r $($*_EXTRA_FILES) $(PWD)/bin/ios-sim/test-$*.app/; fi
	ninja -C obj/ios-sim/test-$*.app -v

ifdef LLVM
APPBUILDER_ARGS += --llvm
endif

build-ios-dev-%: appbuilder.exe test-runner.exe runtime/libmonoios.a $($*_ASSEMBLIES)
	mono appbuilder.exe $(APPBUILDER_ARGS) --target ios-dev64 --mono-sdkdir $(PWD)/../out --appdir $(PWD)/bin/ios-dev/test-$*.app --runtimedir $(PWD)/runtime --builddir obj/ios-dev/test-$*.app --bundle-executable test-$* --bundle-identifier com.xamarin.mono.ios.test-$* --bundle-name test-$* --exe test-runner.exe $(patsubst %,-r %,$(TEST_ASSEMBLIES) $($*_ASSEMBLIES))
	mkdir -p $(PWD)/bin/ios-dev/test-$*.app
	if test "x$($_EXTRA_FILES)" != "x"; then cp -r $($*_EXTRA_FILES) $(PWD)/bin/ios-dev/test-$*.app/; fi
	ninja -C obj/ios-dev/test-$*.app -v

# Install % on the simulator
install-ios-sim-%:
	xcrun simctl install $(SIM_NAME) bin/ios-sim/test-$*.app

# Clean %
clean-ios-sim-%:
	$(RM) -rf obj/ios-sim/test-$*.app bin/ios-sim/test-$*.app

clean-ios-dev-%:
	$(RM) -rf obj/ios-dev/test-$*.app bin/ios-dev/test-$*.app

# Install and run % on the simulator with args $(ARGS) %_ARGS
run-ios-sim-%: harness.exe
	mono harness.exe --start-sim
	xcrun simctl install $(SIM_NAME) bin/ios-sim/test-$*.app
	mono harness.exe --run-sim --logfile ios-sim-$*.log --bundle-id com.xamarin.mono.ios.test-$* --bundle-dir bin/ios-sim/test-$*.app $(ARGS) $($*_ARGS)

clean:
	$(MAKE) -C runtime clean
	$(RM) -rf bin obj *.exe *.log

# Compile all the test assemblies
compile-tests:
	for suite in $(TEST_SUITES); do make -C ../../mcs/class/$$suite PROFILE=monotouch test || exit 1; done

run-ios-sim-all:
	for suite in $(TEST_SUITES); do make build-ios-sim-$$suite || exit 1; make run-ios-sim-$$suite || exit 1; done

build-ios-dev-all:
	for suite in $(TEST_SUITES); do make clean-ios-dev-$$suite; make build-ios-dev-$$suite || exit 1; done

build-ios-dev-llvm-all:
	for suite in $(TEST_SUITES); do make clean-ios-dev-$$suite; make build-ios-dev-$$suite LLVM=1 || exit 1; done

# Developer targets, ignore
# 'launch' doesn't propagate the error code
# With ios11, --console doesn't work any more, it makes the app deadlock
run-ios-sim-direct-%:
	xcrun simctl terminate $(SIM_NAME) com.xamarin.mono.ios.test-$*
	xcrun simctl launch $(SIM_NAME) com.xamarin.mono.ios.test-$* $(ARGS) $($*_ARGS)
	log stream --level debug --predicate 'senderImagePath contains "$*"' --style compact

create-sim:
	xcrun simctl create $(SIM_NAME) 'iPhone 7' com.apple.CoreSimulator.SimRuntime.iOS-11-1

start-sim:
	xcrun simctl boot $(SIM_NAME)

stop-sim:
	xcrun simctl shutdown $(SIM_NAME)

# CONNSTR will be replace by the harness with the real connection string
NUNIT = test-runner.exe CONNSTR -exclude:MobileNotWorking,NotOnMac,NotWorking,ValueAdd,CAS,InetAccess,NotWorkingLinqInterpreter -labels
TESTDIR = $(BCL_DIR)/tests

# Options for each test
Mono.Runtime.Tests_ASSEMBLIES = $(TESTDIR)/monotouch_Mono.Runtime.Tests_test.dll
Mono.Runtime.Tests_ARGS = $(NUNIT) monotouch_Mono.Runtime.Tests_test.dll
corlib_ASSEMBLIES = $(TESTDIR)/monotouch_corlib_test.dll
corlib_EXTRA_FILES = ../../mcs/class/corlib/{es-ES,nn-NO}
corlib_ARGS = $(NUNIT) monotouch_corlib_test.dll
System.Core_ASSEMBLIES = $(TESTDIR)/monotouch_System.Core_test.dll
System.Core_ARGS = $(NUNIT) monotouch_System.Core_test.dll
System_ASSEMBLIES = $(TESTDIR)/monotouch_System_test.dll $(BCL_DIR)/Mono.Security.dll
System_ARGS = $(NUNIT) monotouch_System_test.dll
System.Data_ASSEMBLIES = $(BCL_DIR)/System.Data.dll $(BCL_DIR)/System.Transactions.dll $(TESTDIR)/monotouch_System.Data_test.dll
System.Data_EXTRA_FILES = ../../mcs/class/System.Data/Test
System.Data_ARGS = $(NUNIT) monotouch_System.Data_test.dll
System.Net.Http_ASSEMBLIES = $(BCL_DIR)/System.Net.Http.dll $(TESTDIR)/monotouch_System.Net.Http_test.dll
System.Net.Http_ARGS = $(NUNIT) monotouch_System.Net.Http_test.dll
System.Numerics_ASSEMBLIES = $(TESTDIR)/monotouch_System.Numerics_test.dll
System.Numerics_ARGS = $(NUNIT) monotouch_System.Numerics_test.dll
System.Runtime.Serialization_ASSEMBLIES = $(BCL_DIR)/System.Runtime.Serialization.dll $(BCL_DIR)/System.ServiceModel.dll $(BCL_DIR)/System.ServiceModel.Internals.dll $(TESTDIR)/monotouch_System.Runtime.Serialization_test.dll
System.Runtime.Serialization_ARGS = $(NUNIT) monotouch_System.Runtime.Serialization_test.dll
System.Transactions_ASSEMBLIES = $(BCL_DIR)/System.Transactions.dll $(TESTDIR)/monotouch_System.Transactions_test.dll
System.Transactions_ARGS = $(NUNIT) monotouch_System.Transactions_test.dll
System.IO.Compression_ASSEMBLIES = $(BCL_DIR)/System.IO.Compression.dll $(TESTDIR)/monotouch_System.IO.Compression_test.dll
System.IO.Compression_ARGS = $(NUNIT) monotouch_System.IO.Compression_test.dll
System.IO.Compression_EXTRA_FILES = ../../mcs/class/System.IO.Compression/{archive.zip,test.nupkg}
System.IO.Compression.FileSystem_ASSEMBLIES = $(BCL_DIR)/System.IO.Compression.FileSystem.dll $(BCL_DIR)/System.IO.Compression.dll $(TESTDIR)/monotouch_System.IO.Compression.FileSystem_test.dll
System.IO.Compression.FileSystem_ARGS = $(NUNIT) monotouch_System.IO.Compression.FileSystem_test.dll
System.IO.Compression.FileSystem_EXTRA_FILES = ../../mcs/class/System.IO.Compression.FileSystem/foo
Mono.CSharp_ASSEMBLIES = $(BCL_DIR)/Mono.CSharp.dll $(TESTDIR)/monotouch_Mono.CSharp_test.dll
Mono.CSharp_ARGS = $(NUNIT) monotouch_Mono.CSharp_test.dll
System.Json_ASSEMBLIES = $(BCL_DIR)/System.Json.dll $(TESTDIR)/monotouch_System.Json_test.dll
System.Json_ARGS = $(NUNIT) monotouch_System.Json_test.dll
System.ComponentModel.DataAnnotations_ASSEMBLIES = $(BCL_DIR)/System.ComponentModel.DataAnnotations.dll $(TESTDIR)/monotouch_System.ComponentModel.DataAnnotations_test.dll
System.ComponentModel.DataAnnotations_ARGS = $(NUNIT) monotouch_System.ComponentModel.DataAnnotations_test.dll
Mono.Data.Sqlite_ASSEMBLIES = $(BCL_DIR)/Mono.Data.Sqlite.dll $(TESTDIR)/monotouch_Mono.Data.Sqlite_test.dll
Mono.Data.Sqlite_ARGS = $(NUNIT) monotouch_Mono.Data.Sqlite_test.dll
Mono.Data.Tds_ASSEMBLIES = $(BCL_DIR)/Mono.Data.Tds.dll $(TESTDIR)/monotouch_Mono.Data.Tds_test.dll
Mono.Data.Tds_ARGS = $(NUNIT) monotouch_Mono.Data.Tds_test.dll
System.Security_ASSEMBLIES = $(BCL_DIR)/System.Security.dll $(TESTDIR)/monotouch_System.Security_test.dll
System.Security_ARGS = $(NUNIT) monotouch_System.Security_test.dll
System.Xml_ASSEMBLIES = $(BCL_DIR)/System.Xml.dll $(TESTDIR)/monotouch_System.Xml_test.dll
System.Xml_ARGS = $(NUNIT) monotouch_System.Xml_test.dll
System.Xml.Linq_ASSEMBLIES = $(BCL_DIR)/System.Xml.Linq.dll $(TESTDIR)/monotouch_System.Xml.Linq_test.dll
System.Xml.Linq_ARGS = $(NUNIT) monotouch_System.Xml.Linq_test.dll
Mono.Security_ASSEMBLIES = $(TESTDIR)/monotouch_Mono.Security_test.dll
Mono.Security_ARGS = $(NUNIT) monotouch_Mono.Security_test.dll
System.ServiceModel.Web_ASSEMBLIES = $(BCL_DIR)/System.ServiceModel.Web.dll $(BCL_DIR)/System.ServiceModel.dll $(BCL_DIR)/System.ServiceModel.Internals.dll $(BCL_DIR)/System.IdentityModel.dll $(BCL_DIR)/System.Runtime.Serialization.dll $(TESTDIR)/monotouch_System.ServiceModel.Web_test.dll
System.ServiceModel.Web_ARGS = $(NUNIT) monotouch_System.ServiceModel.Web_test.dll
System.Web.Services_ASSEMBLIES = $(BCL_DIR)/System.Web.Services.dll $(TESTDIR)/monotouch_System.Web.Services_test.dll
System.Web.Services_ARGS = $(NUNIT) monotouch_System.Web.Services_test.dll
