using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using NUnitLite.Runner;
using NUnit.Framework.Internal;
using NUnit.Framework.Api;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public class Driver {
	static void Main () {
		Console.WriteLine ("hello");
		Send ("run", "mini");
	}


	static int step_count, tp_pump_count;
	static Task cur_task;

	static void TPStart () {
		var l = new List<Task> ();
		for (int i = 0; i < 5; ++i) {
			l.Add (Task.Run (() => {
				++step_count;
			}));
			l.Add (Task.Factory.StartNew (() => {
				++step_count;
			}, TaskCreationOptions.LongRunning));
		}
		cur_task = Task.WhenAll (l).ContinueWith (t => {
		});
	}

	static bool TPPump () {
		if (tp_pump_count > 10) {
			Console.WriteLine ("Pumped the TP test 10 times and no progress <o> giving up");
			latest_test_result = "FAIL";
			return false;
		}

		tp_pump_count++;
		latest_test_result = "PASS";
		return !cur_task.IsCompleted;
	}


	static Action dele;
	static IAsyncResult dele_result;
	static void BeginSomething () {
	}

	static void DeleStart ()
	{
		dele = new Action (BeginSomething);
		dele_result = dele.BeginInvoke (null, null);
	}

	static bool DelePump ()
	{
		if (dele_result.IsCompleted) {
			dele.EndInvoke (dele_result);
			return false;
		}
		return true;
	}


	static int fin_count;
	interface IFoo {}
	class Foo : IFoo {
		~Foo () {
			++fin_count;
		}
	}
	static void GcStart ()
	{
		IFoo[] arr = new IFoo [10];
		Volatile.Write (ref arr [1], new Foo ());
		for (int i = 0; i < 100; ++i) {
			var x = new Foo ();
		}
	}

	static bool GcPump ()
	{
		GC.Collect ();
		return fin_count < 100;
	}

	static bool timer_called;
	static int pump_count;

	static void TimerStart () {
		Timer t = new Timer ((_) => {
			timer_called = true;
		});
		t.Change (10, Timeout.Infinite);
		latest_test_result = "EITA";
	}

	static bool TimerPump () {
		++pump_count;
		if (pump_count > 5 || timer_called) {
			latest_test_result = timer_called ? "PASS" : "FAIL";
			return false;
		}

		return true;
	}

	static int run_count;
	public static string Send (string key, string val) {
		if (key == "start-test") {
			StartTest (val);
			return "SUCCESS";
		}
		if (key == "pump-test") {
			return PumpTest (val) ? "IN-PROGRESS" : "DONE" ;
		}
		if (key == "test-result") {
			return latest_test_result;
		}

		return "INVALID-KEY";
	}

	public class TestSuite {
		public string Name { get; set; }
		public string File { get; set; }
	}

	static TestSuite[] suites = new TestSuite [] {
		new TestSuite () { Name = "mini", File = "managed/mini_tests.dll" },
		new TestSuite () { Name = "binding", File = "managed/binding_tests.dll" },
		new TestSuite () { Name = "corlib", File = "managed/wasm_corlib_test.dll" },
		new TestSuite () { Name = "system", File = "managed/wasm_System_test.dll" },
		new TestSuite () { Name = "system-core", File = "managed/wasm_System.Core_test.dll" },
	};

	static IncrementalTestRunner testRunner;
	static string latest_test_result;

	public static bool PumpTest (string name) {
		if (name == "tp")
			return TPPump ();
		if (name == "dele")
			return DelePump ();
		if (name == "gc")
			return GcPump ();
		if (name == "timer")
			return TimerPump ();

		if (testRunner == null)
			return false;
		try {
			bool res = testRunner.Step ();
			if (!res) {
				latest_test_result = testRunner.Status;
				testRunner = null;
			}
			return res;
		} catch (Exception e) {
			Console.WriteLine (e);
			latest_test_result = "FAIL";
			return true;
		}
	}

	public static void StartTest (string name) {
		var baseDir = AppDomain.CurrentDomain.BaseDirectory;
		if (testRunner != null)
			throw new Exception ("Test in progress");

		if (name == "tp") {
			TPStart ();
			return;
		}
		if (name == "dele") {
			DeleStart ();
			return;
		}
		if (name == "gc") {
			GcStart ();
			return;
		}
		if (name == "timer") {
			TimerStart ();
			return;
		}

		string extra_disable = "";
		latest_test_result = "IN-PROGRESS";

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

		testRunner.Exclude ("NotWasm,WASM,NotWorking,ValueAdd,CAS,InetAccess,NotWorkingRuntimeInterpreter,MultiThreaded,StackWalk,GetCallingAssembly");
		testRunner.Add (Assembly.LoadFrom (baseDir + "/" + testsuite_name));
		// testRunner.RunOnly ("MonoTests.System.Threading.AutoResetEventTest.MultipleSet");

		// This is useful if you need to skip to the middle of a huge test suite like corlib.
		// testRunner.SkipFirst (4550);
		testRunner.Start (10);
	}

}
