using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Mono.Options;

public class Harness
{
	public const string SIM_NAME = "xamarin.ios-sdk.sim";

	static void Usage () {
		Console.WriteLine ("Usage: mono harness.exe <options> <app dir>");
		Console.WriteLine ("Where options are:");
		Console.WriteLine ("\t--run-sim");
		Console.WriteLine ("\t--app=<app bundle id>");
		Console.WriteLine ("\t--logfile=<log file name>");
	}

	public static int Main (string[] args) {
		new Harness ().Run (args);
		return 0;
	}

	string bundle_id;
	string bundle_dir;
	string logfile_name;
	string[] new_args;

	public void Run (string[] args) {
		string action = "";
		bundle_id = "";
		bundle_dir = "";
		logfile_name = "";

		var p = new OptionSet () {
		  { "start-sim", s => action = "start-sim" },
		  { "run-sim", s => action = "run-sim" },
		  { "bundle-id=", s => bundle_id = s },
		  { "bundle-dir=", s => bundle_dir = s },
		  { "logfile=", s => logfile_name = s },
		};
		new_args = p.Parse (args).ToArray ();

		if (action == "start-sim") {
			StartSim ();
		} else if (action == "run-sim") {
			if (bundle_id == "" || bundle_dir == "") {
				Console.WriteLine ("The --bundle-id and --bundle-dir arguments are mandatory.");
				Environment.Exit (1);
			}
			RunSim ();
		} else {
			Usage ();
			Environment.Exit (1);
		}
	}

	void StartSim () {
		// Check whenever our simulator instance exists
		var args = "simctl list devices";
		Console.WriteLine ("Running: " + "xcrun " + args);
		var start_info = new ProcessStartInfo ("xcrun", args);
		start_info.RedirectStandardOutput = true;
		start_info.UseShellExecute = false;
		var process = Process.Start (start_info);
		var stream = process.StandardOutput;
		string line = "";
		string state_line = "";
		while (true) {
			line = stream.ReadLine ();
			if (line == null)
				break;
			if (line.Contains (SIM_NAME)) {
				state_line = line;
				break;
			}
		}
		process.WaitForExit ();
		if (process.ExitCode != 0)
			Environment.Exit (1);

		bool need_start = false;
		if (state_line == "") {
			// Get the runtime type
			args = "simctl list runtimes";
			Console.WriteLine ("Running: " + "xcrun " + args);
			start_info = new ProcessStartInfo ("xcrun", args);
			start_info.RedirectStandardOutput = true;
			start_info.UseShellExecute = false;
			process = Process.Start (start_info);
			stream = process.StandardOutput;
			string ios_line = null;
			while (true) {
				line = stream.ReadLine ();
				if (line == null)
					break;
				if (line.Contains ("com.apple.CoreSimulator.SimRuntime.iOS")) {
					ios_line = line;
					break;
				}
			}
			process.WaitForExit ();
			if (process.ExitCode != 0)
				Environment.Exit (1);
			if (ios_line == null) {
				Console.WriteLine ("Unable to parse process output.");
				Environment.Exit (1);
			}
			string runtime = line.Substring (line.IndexOf ("com.apple.CoreSimulator.SimRuntime.iOS"));

			// Create the simulator
			args = "simctl create " + SIM_NAME + " 'iPhone 7' " + runtime;
			Console.WriteLine ("Running: " + "xcrun " + args);
			process = Process.Start ("xcrun", args);
			process.WaitForExit ();
			if (process.ExitCode != 0)
				Environment.Exit (1);
			need_start = true;
		} else if (state_line.Contains ("(Shutdown)")) {
			need_start = true;
		}

		if (need_start) {
			args = "simctl boot " + SIM_NAME;
			Console.WriteLine ("Running: " + "xcrun " + args);
			process = Process.Start ("xcrun", args);
			process.WaitForExit ();
			if (process.ExitCode != 0)
				Environment.Exit (1);
		}
	}

	void RunSim () {
		Console.WriteLine ("App: " + bundle_id);

		StartSim ();

		// Install the app
		// We do this all the time since its cheap
		string exe = "xcrun";
		string args = "simctl install " + SIM_NAME + " " + bundle_dir;
		Console.WriteLine ("Running: " + exe + " " + args);
		var process = Process.Start (exe, args);
		process.WaitForExit ();
		if (process.ExitCode != 0)
			Environment.Exit (1);

		//
		// Instead of returning test results using an extra socket connection,
		// simply read and parse the app output through the osx log facility,
		// since stdout from the app goes to logger. This allows us to
		// use the stock nunit-lite test runner.
		//

		// Start a process to read the app output through the osx log facility
		// The json output would be easier to parse, but its emitted in multi-line mode,
		// and the ending } is only emitted with the beginning of the next entry, so its
		// not possible to parse it in streaming mode.
		// We start this before the app to prevent races
		var app_name = bundle_id.Substring (bundle_id.LastIndexOf ('.') + 1);
		var logger_args = "stream --level debug --predicate 'senderImagePath contains \"" + app_name + "\"' --style compact";
		Console.WriteLine ("Running: " + "log " + logger_args);
		var start_info = new ProcessStartInfo ("log", logger_args);
		start_info.RedirectStandardOutput = true;
		start_info.RedirectStandardError = true;
		start_info.UseShellExecute = false;
		var log_process = Process.Start (start_info);

		string app_args = "";
		foreach (var a in new_args)
			app_args += a + " ";

		// Terminate previous app
		exe = "xcrun";
		args = "simctl terminate " + SIM_NAME + " " + bundle_id;
		Console.WriteLine ("Running: " + exe + " " + args);
		process = Process.Start (exe, args);
		process.WaitForExit ();
		if (process.ExitCode != 0) {
			log_process.Kill ();
			Environment.Exit (1);
		}

		// Launch new app
		exe = "xcrun";
		args = "simctl launch " + SIM_NAME + " " + bundle_id + " " + app_args;
		Console.WriteLine ("Running: " + exe + " " + args);
		process = Process.Start (exe, args);
		process.WaitForExit ();
		if (process.ExitCode != 0) {
			log_process.Kill ();
			Environment.Exit (1);
		}

		//
		// Read the test results from the app output
		//
		TextWriter w = new StreamWriter (logfile_name);
		string result_line = null;
		var stream = log_process.StandardOutput;
		while (true) {
			string line = stream.ReadLine ();
			if (line == null)
				break;
			// Extract actual output
			// The lines look like:
			// 2017-11-28 14:45:16.203 Df test-corlib[5018:20c89] ***** MonoTests.System.UInt16Test.ToString_Defaults
			// except if the output contains newlines.
			if (line.Contains (app_name + "[")) {
				int pos = line.IndexOf (' ');
				pos = line.IndexOf (' ', pos + 1);
				pos = line.IndexOf (' ', pos + 1);
				pos = line.IndexOf (' ', pos + 1);
				line = line.Substring (pos + 1);
			}

			Console.WriteLine (line);
			w.WriteLine (line);
			if (line.Contains ("Tests run:"))
				result_line = line;
			// Printed by the runtime
			if (line.Contains ("Exit code:"))
				break;
		}

		log_process.Kill ();
		log_process.WaitForExit ();

		if (result_line != null && result_line.Contains ("Errors: 0"))
			Environment.Exit (0);
		else
			Environment.Exit (1);
	}
}
