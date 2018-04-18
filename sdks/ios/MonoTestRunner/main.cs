using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using NUnitLite.Runner;
using NUnit.Framework.Internal;
using NUnit.Framework.Api;

public class MyRunner : TextUI, ITestListener
{
	public String failed_tests = "";

    public void TestFinished(ITestResult result)
	{
		if (result.ResultState.Status == TestStatus.Failed) {
			if (failed_tests.Length > 0)
				failed_tests += ", ";
			failed_tests += result.Test.Name;
		}
		base.TestFinished (result);
	}
}

public class Driver {
	static MyRunner runner;
	static Thread runner_thread;
	static bool done;
	public static string Send (string key, string value) {
		if (key == "start") {
			if (runner != null)
				return "IN-PROGRESS";
			StartTest (value);
			return "STARTED";
		} else if (key == "status") {
			if (!done)
				return runner == null ? "NO RUN" : "IN-PROGRESS";
			done = false;
			runner_thread.Join ();
			runner_thread = null;

			var local_runner = runner;
			runner = null;
			return local_runner.Failure ? ("FAIL: " + local_runner.failed_tests): "PASS";
		} else {
			return "WTF";
		}
	}

	public class TestSuite {
		public string Name { get; set; }
		public string File { get; set; }
	}

	static TestSuite[] suites = new TestSuite [] {
		new TestSuite () { Name = "mini", File = "mini_tests.dll" },
		new TestSuite () { Name = "corlib", File = "monotouch_corlib_test.dll" },
		new TestSuite () { Name = "system", File = "monotouch_System_test.dll" },
	};

	public static void StartTest (string name) {
		var baseDir = AppDomain.CurrentDomain.BaseDirectory;
		// name = "system,MonoTests.System.Diagnostics.ProcessTest";
		name = "corlib";

		string extra_disable = "";
		// if (IntPtr.Size == 4)
		// 	extra_disable = ",LargeFileSupport";

		// extra_disable += ",AndroidNotWorking";
		string[] args = name.Split (',');
		var testsuite_name = suites.Where (ts => ts.Name == args [0]).Select (ts => ts.File).FirstOrDefault ();
		if (testsuite_name == null)
			throw new Exception ("NO SUITE NAMED " + args [0]);

		string test_name = null;
		int? range = null;
		for (int i = 1; i < args.Length; ++i) {
			int r;
			if (int.TryParse (args [i], out r))
				range = r;
			else
				test_name = args [i];
		}

		var arg_list = new List<string> ();
		arg_list.Add ("-labels");
		if (test_name != null)
			arg_list.Add ("-test=" + test_name);

		arg_list.Add ("-exclude=NotOnMac,NotWorking,ValueAdd,CAS,InetAccess,MobileNotWorking,SatelliteAssembliesNotWorking" + extra_disable);
		arg_list.Add (baseDir + "/" + testsuite_name);

		done = false;
		runner = new MyRunner ();
		runner_thread = new Thread ( () => {
			runner.Execute (arg_list.ToArray ());
			done = true;
		});
		runner_thread.Start ();
	}

	public static int Main () {
		return 1;
	}
}
