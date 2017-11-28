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
