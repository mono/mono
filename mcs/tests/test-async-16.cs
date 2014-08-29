using System;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

class Base : IDisposable
{
	protected static int dispose_counter;

	public void Dispose ()
	{
		++dispose_counter;
	}
}

class Tester : Base
{
	async Task<int> SwitchTest_1 ()
	{
		switch (await Task.Factory.StartNew (() => "X").ConfigureAwait (false)) {
		case "A":
			return 1;
		case "B":
			return 2;
		case "C":
			return 3;
		case "D":
			return 4;
		case "X":
			return 0;
		}

		return 5;
	}

	async Task<int> Using_1 ()
	{
		using (Base a = await Task.Factory.StartNew (() => new Base ()).ConfigureAwait (false),
				b = await Task.Factory.StartNew (() => new Tester ()).ConfigureAwait (false),
				c = await Task.Factory.StartNew (() => new Base ()).ConfigureAwait (false),
				d = await Task.Factory.StartNew (() => new Base ()).ConfigureAwait (false)) {
		}

		if (dispose_counter != 4)
			return 1;

		return 0;
	}

	async Task<int> Foreach_1 ()
	{
		int total = 0;
		foreach (var e in await Task.Factory.StartNew (() => new List<int> () { 1, 2, 3 }).ConfigureAwait (false)) {
			total += e;
		}

		return total - 6;
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

			var ti = t as Task<int>;
			if (ti != null) {
				if (ti.Result != 0) {
					Console.WriteLine ("FAILED (Result={0})", ti.Result);
					return false;
				}
			} else {
				var tb = t as Task<bool>;
				if (tb != null) {
					if (!tb.Result) {
						Console.WriteLine ("FAILED (Result={0})", tb.Result);
						return false;
					}
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
