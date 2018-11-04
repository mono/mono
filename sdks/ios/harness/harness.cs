using System;
using System.IO;
using System.Json;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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
		  { "run-dev", s => action = "run-dev" },
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
		} else if (action == "run-dev") {
			if (bundle_dir == "") {
				Console.WriteLine ("The --bundle-dir argument is mandatory.");
				Environment.Exit (1);
			}
			RunDev ();
		} else {
			Usage ();
			Environment.Exit (1);
		}
	}

	void StartSim () {
		// Check whenever our simulator instance exists
		string state_line = "";
		{
			var args = "simctl list devices";
			Console.WriteLine ("Running: " + "xcrun " + args);
			var start_info = new ProcessStartInfo ("xcrun", args);
			start_info.RedirectStandardOutput = true;
			start_info.UseShellExecute = false;
			var process = Process.Start (start_info);
			var stream = process.StandardOutput;
			string line = "";
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
		}

		if (state_line.Contains ("unavailable")) {
			// Created for an older version of xcode
			var args = "simctl delete unavailable";
			Console.WriteLine ("Running: " + "xcrun " + args);
			var process = Process.Start ("xcrun", args);
			process.WaitForExit ();
			state_line = "";
		}

		bool need_start = false;
		if (state_line == "") {
			// Get the runtime type
			var args = "simctl list -j runtimes";
			Console.WriteLine ("Running: " + "xcrun " + args);
			var start_info = new ProcessStartInfo ("xcrun", args);
			start_info.RedirectStandardOutput = true;
			start_info.UseShellExecute = false;
			var process = Process.Start (start_info);
			var stream = process.StandardOutput;
			JsonObject value = JsonValue.Parse (stream.ReadToEnd ()) as JsonObject;
			string runtime = value ["runtimes"][0]["identifier"];

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
			var args = "simctl boot " + SIM_NAME;
			Console.WriteLine ("Running: " + "xcrun " + args);
			var process = Process.Start ("xcrun", args);
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
		// Test results are returned using an socket connection.
		//
		var host = Dns.GetHostEntry (Dns.GetHostName ());
		var server = new TcpListener (System.Net.IPAddress.Loopback, 0);
		server.Start ();
		int port = ((IPEndPoint)server.LocalEndpoint).Port;

		string app_args = "";
		foreach (var a in new_args)
			app_args += a + " ";
		if (!app_args.Contains ("CONNSTR"))
			throw new Exception ();
		app_args = app_args.Replace ("CONNSTR", $"tcp:localhost:{port}");

		// Terminate previous app
		exe = "xcrun";
		args = "simctl terminate " + SIM_NAME + " " + bundle_id;
		Console.WriteLine ("Running: " + exe + " " + args);
		process = Process.Start (exe, args);
		process.WaitForExit ();
		if (process.ExitCode != 0)
			Environment.Exit (1);

		// Launch new app
		exe = "xcrun";
		args = "simctl launch " + SIM_NAME + " " + bundle_id + " " + app_args;
		Console.WriteLine ("Running: " + exe + " " + args);
		process = Process.Start (exe, args);
		process.WaitForExit ();
		if (process.ExitCode != 0)
			Environment.Exit (1);

		//
		// Read test results from the tcp connection
		//
		TextWriter w = new StreamWriter (logfile_name);
		string result_line = null;
		var client = server.AcceptTcpClient ();
		var stream = client.GetStream ();
		var reader = new StreamReader (stream);
		while (true) {
			var line = reader.ReadLine ();
			if (line == null)
				break;
			Console.WriteLine (line);
			w.WriteLine (line);
			if (line.Contains ("Tests run:"))
				result_line = line;
			// Printed by the runtime
			if (line.Contains ("Exit code:"))
				break;
		}

		if (result_line != null && result_line.Contains ("Errors: 0") && result_line.Contains ("Failures: 0"))
			Environment.Exit (0);
		else
			Environment.Exit (1);
	}

	void RunDev () {
		Console.WriteLine ("App: " + bundle_dir);

		//
		// Test results are returned using an socket connection.
		//
		var host = Dns.GetHostEntry (Dns.GetHostName ());
		var hostip = host.AddressList [0].ToString ();
		Console.WriteLine ("Host ip: " + hostip);
		var server = new TcpListener (host.AddressList [0], 0);
		server.Start ();
		int port = ((IPEndPoint)server.LocalEndpoint).Port;

		string app_args = "";
		foreach (var a in new_args)
			app_args += a + " ";
		if (!app_args.Contains ("CONNSTR"))
			throw new Exception ();
		app_args = app_args.Replace ("CONNSTR", $"tcp:{hostip}:{port}");

		// Launch new app
		//
		// -v be verbose
		// -b bundle dir
		// -a args
		// -u unbuffered stdout
		// -L launch app
		//
		string exe = "ios-deploy";
		string args = "-v -L -u -b " + bundle_dir + " -a '" + app_args + "'";
		Console.WriteLine ("Running: " + exe + " " + args);
		var process = Process.Start (exe, args);
		process.WaitForExit ();
		if (process.ExitCode != 0)
			Environment.Exit (1);

		//
		// Read test results from the tcp connection
		//
		TextWriter w = new StreamWriter (logfile_name);
		string result_line = null;

		Console.WriteLine ("*** test-runner output ***");

		int wait_time = 0;
		while (!server.Pending ()) {
			wait_time += 100;
			if (wait_time == 10 * 1000) {
				Console.Error.WriteLine ("Timed out waiting for test runner to connect.");
				Environment.Exit (1);
			}
			Thread.Sleep (100);
		}
		var client = server.AcceptTcpClient ();
		var stream = client.GetStream ();
		var reader = new StreamReader (stream);

		while (true) {
			var line = reader.ReadLine ();
			if (line == null)
				break;
			Console.WriteLine (line);
			w.WriteLine (line);
			if (line.Contains ("Tests run:"))
				result_line = line;
			// Printed by the runtime
			if (line.Contains ("Exit code:"))
				break;
		}

		if (result_line != null && result_line.Contains ("Errors: 0") && result_line.Contains ("Failures: 0"))
			Environment.Exit (0);
		else
			Environment.Exit (1);
	}
}
