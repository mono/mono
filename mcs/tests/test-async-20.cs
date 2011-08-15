// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

// Dynamic tests

class Base
{
	public int Value;
}

class Tester : Base
{
	async Task<bool> Add_1 ()
	{
		dynamic d = 1;
		int total = d + await Task.Factory.StartNew (() => 2);
		return total == 3;
	}

	async Task<bool> AssignCompound_1 ()
	{
		dynamic d = new Base ();
		d.Value = 3;
		d.Value += await Task.Factory.StartNew (() => 2);
		return d.Value == 5;
	}

	async Task<bool> Convert_1 ()
	{
		string s = await Task.Factory.StartNew (() => (dynamic) "x");
		return s == "x";
	}

	async Task<bool> Invocation_1 ()
	{
		var r = (await Task.Factory.StartNew (() => (dynamic) "x|y|z")).Split ('|');
		return r[2] == "z";
	}

	static bool RunTest (MethodInfo test)
	{
		Console.Write ("Running test {0, -25}", test.Name);
		try {
			Task t = test.Invoke (new Tester (), null) as Task;
			if (!Task.WaitAll (new[] { t }, 1000)) {
				Console.WriteLine ("FAILED (Timeout)");
				return false;
			}

			var tb = t as Task<bool>;
			if (tb != null) {
				if (!tb.Result) {
					Console.WriteLine ("FAILED (Result={0})", tb.Result);
					return false;
				}
			}

			Console.WriteLine ("OK");
			return true;
		} catch (Exception e) {
			Console.WriteLine ("FAILED");
			Console.WriteLine (e.ToString ());
			return false;
		}
	}

	public static int Main ()
	{
		var tests = from test in typeof (Tester).GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
					where test.GetParameters ().Length == 0
					orderby test.Name
					select RunTest (test);

		int failures = tests.Count (a => !a);
		Console.WriteLine (failures + " tests failed");
		return failures;
	}
}
