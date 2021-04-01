# The test runner

The test runner is an objective-c app which embeds the runtime. It has a command line interface similar to the mono runtime, i.e. `<exe> <arguments>`.

# The test harness

The test harness is a C# app which automates running a test suite. It
install the app on the simulator, runs it, and collects output into
a log file.

# The app builder

This is a C# app which is used to create an ios .app bundle.

# Make targets

Similar to the ones in xamarin-macios/tests

	* *action*-*what*-*where*-*project*

* Action

	* build-
	* install-
	* run-

* What

	* -ios-

* Where

	* -sim-
	* -dev-

* Project

	* corlib etc.

The test apps require the corresponding test assembly to be already
built, i.e. by running make PROFILE=monotouch test in mcs/class/corlib
etc.

# Running tests on device

* The test apps need to be signed using a real signing identity instead
of the default ad-hoc signing. It also needs to include a provisioning
profile. This can be done using:

```
make build-ios-dev-<app> IOS_SIGNING_IDENTITY="iPhone Developer: XXX" IOS_TEAM_IDENTIFIER="XXXX"
```

* The certificates/provisioning profiles need to be installed on the
host/device. Check `$HOME/Library/MobileDevice/Provisioning Profiles/*.mobileprovision` for a team identifier you can use.

* The app is installed/run using 'ios-deploy', make sure it is installed.

* The host and the device needs to be on the same network. This is
needed because the test results are sent back over a tcp connection.

Other possible approaches for returning results:

* Using the ios os_log facility. Unfortunately, the command line 'log' tool cannot
seem to read the device logs, only the graphical Console app can.
* Using `idevicesyslog` from `libimobiledevice`.
* Have the app listen on a port, and have the test harness connect to it using
`libimobiledevice`, i.e. https://github.com/rsms/peertalk.
* Use a publish-subscribe pattern by uploading test results to some cloud service like
Azure EventHub. This only requires client side internet access on the device and
the test harness.
