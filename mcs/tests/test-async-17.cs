// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Linq;

class Tester
{
	async Task<int> TestException_1 ()
	{
		await Task.Factory.StartNew (() => { throw new ApplicationException (); });
		return 1;
	}

	async Task TestException_2 ()
	{
		await Task.Factory.StartNew (() => { throw new ApplicationException (); });
	}

	async Task TestException_3 ()
	{
		Func<Task> a = async () => await Task.Factory.StartNew (() => { throw new ApplicationException (); });
		await a ();
	}
	
	async Task<int> TestException_4 ()
	{
		try {
			await Task.Factory.StartNew (() => 5);
		} finally {
			throw new ApplicationException ();
		}
	}
	
	static bool RunTest (MethodInfo test)
	{
		Console.Write ("Running test {0, -25}", test.Name);
		try {
			Task t = test.Invoke (new Tester (), null) as Task;
			try {
				if (!Task.WaitAll (new[] { t }, 1000)) {
					Console.WriteLine ("FAILED (Timeout)");
					return false;
				}
			} catch (AggregateException) {
			}
			
			if (t.Status != TaskStatus.Faulted) {
				Console.WriteLine ("FAILED (Status={0})", t.Status);
				return false;
			}
			
			if (!(t.Exception.InnerException is ApplicationException)) {
				Console.WriteLine ("FAILED with wrong exception");
				return false;
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
