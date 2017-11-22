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


public class MyRunner : TextUI, ITestListener
{
	public String failed_tests = "";
}

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
		if (key == "say") {
			if (val == "hello") {
				return "OK:" + WebAssembly.Runtime.InvokeJS ("1 + 2");
			} else if (val == "js-exception") {
				try {
					return "OK:" + WebAssembly.Runtime.InvokeJS ("throw 1");
				} catch (WebAssembly.JSException e) {
					Console.WriteLine (e.Message);
					return "EH:" + e.Message;
				}
			} else if (val == "sharp-exception") {
				throw new Exception ("error!");
			}
		}

		if (key != "run")
			return "INVALID-ARG";
		if (val == "gc") { 
			Console.WriteLine ("running {0} step", run_count);
			for (int i = 0; i < 1000 * 2; ++i) {
				var x = new object [1000];
			}
			++run_count;
			return run_count >= 10 ? "DONE" :  "IN PROGRESS";
		}
		StartTest (val);
		return "SUCCESS";

		return "FAIL";
	}

	public class TestSuite {
		public string Name { get; set; }
		public string File { get; set; }
	}

	static TestSuite[] suites = new TestSuite [] {
		new TestSuite () { Name = "mini", File = "managed/mini_tests.dll" },
		new TestSuite () { Name = "corlib", File = "monodroid_corlib_test.dll" },
		new TestSuite () { Name = "system", File = "monodroid_System_test.dll" },
	};

	public static void StartTest (string name) {
		var baseDir = AppDomain.CurrentDomain.BaseDirectory;

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

		var arg_list = new List<string> ();
		arg_list.Add ("-labels");
		if (test_name != null)
			arg_list.Add ("-test=" + test_name);

		arg_list.Add ("-exclude=WASM,NotWorking,ValueAdd,CAS,InetAccess");
		arg_list.Add (baseDir + "/" + testsuite_name);

		var runner = new MyRunner ();
		runner.Execute (arg_list.ToArray ());
	}

}
