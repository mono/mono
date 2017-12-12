
all: harness.exe appbuilder.exe test-runner.exe

BCL_DIR = ../../mcs/class/lib/monotouch

TEST_SUITES = \
	Mono.Runtime.Tests \
	corlib \
	System.Core \
	System.Data

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

build-ios-dev-%: appbuilder.exe test-runner.exe runtime/libmonoios.a $($*_ASSEMBLIES)
	mono appbuilder.exe --target ios-dev64 --mono-sdkdir $(PWD)/../out --appdir $(PWD)/bin/ios-dev/test-$*.app --runtimedir $(PWD)/runtime --builddir obj/ios-dev/test-$*.app --bundle-executable test-$* --bundle-identifier com.xamarin.mono.ios.test-$* --bundle-name test-$* --exe test-runner.exe $(patsubst %,-r %,$(TEST_ASSEMBLIES) $($*_ASSEMBLIES))
	mkdir -p $(PWD)/bin/ios-dev/test-$*.app
	if test "x$($_EXTRA_FILES)" != "x"; then cp -r $($*_EXTRA_FILES) $(PWD)/bin/ios-dev/test-$*.app/; fi
	ninja -C obj/ios-dev/test-$*.app -v

# Install % on the simulator
install-ios-sim-%:
	xcrun simctl install $(SIM_NAME) bin/ios-sim/test-$*.app

# Clean %
clean-ios-sim-%:
	$(RM) -rf obj/ios-sim/test-$*.app bin/ios-sim/test-$*.app

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
	for suite in $(TEST_SUITES); do make build-ios-dev-$$suite || exit 1; done

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

NUNIT = test-runner.exe -exclude:MobileNotWorking,NotOnMac,NotWorking,ValueAdd,CAS,InetAccess,NotWorkingLinqInterpreter -labels
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
