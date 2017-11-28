
all: harness.exe

BCL_DIR = ../../mcs/class/lib/monotouch

TEST_SUITES = \
	Mono.Runtime.Tests \
	corlib \
	System.Core \
	System.Data

SIM_NAME = xamarin.ios-sdk.sim

harness.exe: harness/harness.cs harness/Options.cs
	csc /out:$@ $^

# Build % from assemblies %_ASSEMBLIES
# The end result is in bin/ios-sim/test-%.app
build-ios-sim-%: $($*_ASSEMBLIES)
	make -C runtime/
	rm -rf tmp.app
	mkdir -p tmp.app
	cp runtime/test-runner tmp.app/test-$*
	cp runtime/Info.plist.in tmp.app/1
	sed -e "s/BUNDLE_IDENTIFIER/com.xamarin.mono.ios.test-$*/g" < tmp.app/1 > tmp.app/2
	sed -e "s/BUNDLE_EXECUTABLE/test-$*/g" < tmp.app/2 > tmp.app/3
	sed -e "s/BUNDLE_NAME/test-$*/g" < tmp.app/3 > tmp.app/Info.plist
	rm -f tmp.app/{1,2,3}
	plutil -convert binary1 tmp.app/Info.plist
	cp -r runtime/Base.lproj tmp.app
	cp -r $(BCL_DIR)/{mscorlib.dll,System.dll,System.Xml.dll,System.Core.dll,I18N.dll,I18N.West.dll,Mono.Simd.dll,System.Numerics.dll,System.Numerics.Vectors.dll,nunitlite.dll,nunit-lite-console.exe} $($*_ASSEMBLIES) $($*_EXTRA_FILES) tmp.app
	/usr/bin/codesign --force --sign - --timestamp=none tmp.app
	mkdir -p bin/ios-sim
	rm -rf bin/ios-sim/test-$*.app
	mv tmp.app bin/ios-sim/test-$*.app

# Install % on the simulator
install-ios-sim-%:
	xcrun simctl install $(SIM_NAME) bin/ios-sim/test-$*.app

# Clean %
clean-ios-sim-%:
	$(RM) -rf bin/ios-sim/test-$*.app

# Install and run % on the simulator with args $(ARGS) %_ARGS
run-ios-sim-%: harness.exe
	mono harness.exe --start-sim
	xcrun simctl install $(SIM_NAME) bin/ios-sim/test-$*.app
	mono harness.exe --run-sim --logfile ios-sim-$*.log --bundle-id com.xamarin.mono.ios.test-$* --bundle-dir bin/ios-sim/test-$*.app $(ARGS) $($*_ARGS)

clean:
	$(MAKE) -C runtime clean
	$(RM) -rf bin harness.exe *.log

# Compile all the test assemblies
compile-tests:
	for suite in $(TEST_SUITES); do make -C ../../mcs/class/$$suite PROFILE=monotouch test || break; done

run-tests:
	for suite in $(TEST_SUITES); do make build-ios-sim-$$suite || break; make run-ios-sim-$$suite || break; done

# Developer targets, ignore
# 'launch' doesn't propagate the error code
# With ios11, --console doesn't work any more, it makes the app deadlock
run-ios-sim-direct-%:
	xcrun simctl terminate $(SIM_NAME) com.xamarin.mono.ios.test-$*
	xcrun simctl launch $(SIM_NAME) com.xamarin.mono.ios.test-$* $(ARGS) $($*_ARGS)
	log stream --level debug --predicate 'senderImagePath contains "$*"' --style compact

create-sim:
	xcrun simctl create $(SIM_NAME) 'iPhone 8 Plus' com.apple.CoreSimulator.SimRuntime.iOS-11-1

start-sim:
	xcrun simctl boot $(SIM_NAME)

stop-sim:
	xcrun simctl shutdown $(SIM_NAME)

NUNIT = nunit-lite-console.exe -exclude:MobileNotWorking,NotOnMac,NotWorking,ValueAdd,CAS,InetAccess,NotWorkingLinqInterpreter -labels
TESTDIR = $(BCL_DIR)/tests

# Options for each test
Mono.Runtime.Tests_ASSEMBLIES = $(TESTDIR)/monotouch_Mono.Runtime.Tests_test.dll
Mono.Runtime.Tests_ARGS = $(NUNIT) monotouch_Mono.Runtime.Tests_test.dll
corlib_ASSEMBLIES = $(TESTDIR)/monotouch_corlib_test.dll
corlib_EXTRA_FILES = ../../mcs/class/corlib/{es-ES,nn-NO}
corlib_ARGS = $(NUNIT) monotouch_corlib_test.dll
System.Core_ASSEMBLIES = $(TESTDIR)/monotouch_System.Core_test.dll
System.Core_ARGS = $(NUNIT) monotouch_System.Core_test.dll
System_ASSEMBLIES = $(TESTDIR)/monotouch_System_test.dll
System_ARGS = $(NUNIT) monotouch_System_test.dll
System.Data_ASSEMBLIES = $(BCL_DIR)/System.Data.dll $(BCL_DIR)/System.Transactions.dll $(TESTDIR)/monotouch_System.Data_test.dll
System.Data_EXTRA_FILES = ../../mcs/class/System.Data/Test
System.Data_ARGS = $(NUNIT) monotouch_System.Data_test.dll
