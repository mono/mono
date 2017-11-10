using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using NUnitLite.Runner;
using NUnit.Framework.Internal;
using NUnit.Framework.Api;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace WebAssembly {
	public sealed class Runtime {
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string InvokeJS (string str, out int exceptional_result);

		public static string InvokeJS (string str)
		{
			int exception = 0;
			var res = InvokeJS (str, out exception);
			if (exception != 0)
				throw new JSException (res);
			return res;
		}
	}

	public class JSException : Exception {
		public JSException (string msg) : base (msg) {}
	}
}

public class Driver {
	static void Main () {
		Console.WriteLine ("hello");
		Send ("run", "mini");
	}

	static int run_count;
	public static string Send (string key, string val) {
		if (key == "start-test") {
			StartTest (val);
			return "SUCCESS";
		}
		if (key == "pump-test") {
			return PumpTest () ? "IN-PROGRESS" : "DONE" ;
		}

		return "INVALID-KEY";
	}

	public class TestSuite {
		public string Name { get; set; }
		public string File { get; set; }
	}

	static TestSuite[] suites = new TestSuite [] {
		new TestSuite () { Name = "mini", File = "managed/mini_tests.dll" },
		new TestSuite () { Name = "corlib", File = "managed/wasm_corlib_test.dll" },
		new TestSuite () { Name = "system", File = "managed/wasm_System_test.dll" },
	};

	static IncrementalTestRunner testRunner;

	public static bool PumpTest () {
		if (testRunner == null)
			return false;
		try {
			bool res = testRunner.Step ();
			if (!res)
				testRunner = null;
			return res;
		} catch (Exception e) {
			Console.WriteLine (e);
			return true;
		}
	}

	public static void StartTest (string name) {
		var baseDir = AppDomain.CurrentDomain.BaseDirectory;
		if (testRunner != null)
			throw new Exception ("Test in progress");

		string extra_disable = "";

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

		testRunner = new IncrementalTestRunner ();
		// testRunner.PrintLabels ();
		// if (test_name != null)
		// 	testRunner.RunTest (test_name);

		testRunner.Exclude ("WASM,NotWorking,ValueAdd,CAS,InetAccess");
		testRunner.Add (Assembly.LoadFrom (baseDir + "/" + testsuite_name));
		// testRunner.RunOnly ("MonoTests.System.Threading.AutoResetEventTest.MultipleSet");

		testRunner.Start (10);
	}

}
