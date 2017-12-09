# The test runner

The test runner is an objective-c app which embeds the runtime. It has a command line interface similar to the mono runtime, i.e.
<exe> <arguments>.

# The test harness

The test harness is a C# app which automates running a test suite. It
install the app on the simulator, runs it, and collects output into
a log file.

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

* Project

	* corlib etc.

The test apps require the corresponding test assembly to be already
built, i.e. by running make PROFILE=monotouch test in mcs/class/corlib
etc.

# Running tests on device

This is currently not implemented.

The app can be started on the device by using ios-deploy (https://github.com/phonegap/ios-deploy):
`ios-deploy  -d -b bin/ios-dev/test-Mono.Runtime.Tests.app/ -d -a 'nunit-lite-console.exe bin/ios-dev/test-Mono.Runtime.Tests.app/monotouch_Mono.Runtime.Tests_test.dll -labels'`

Getting test results from the app is more complicated. Some possible approaches:
* Using the ios os_log facility. Unfortunately, the command line 'log' tool cannot
seem to read the device logs, only the graphical Console app can.
* Using `idevicesyslog` from `libimobiledevice`.
* Have the app send back results using a tcp connection. This requires starting a
server from the test harness, and passing the address to the app using a command line
option. It also requires the device and the host to be on the same network.
* Have the app listen on a port, and have the test harness connect to it using
`libimobiledevice`, i.e. https://github.com/rsms/peertalk.
* Use a publish-subscribe pattern by uploading test results to some cloud service.
This only requires internet access on the device.

