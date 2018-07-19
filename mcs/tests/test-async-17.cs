using System;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Linq;

class Tester
{
	async Task<int> TestException_1 ()
	{
		await Task.Factory.StartNew (() => { throw new ApplicationException (); }).ConfigureAwait (false);
		return 1;
	}

	async Task TestException_2 ()
	{
		await Task.Factory.StartNew (() => { throw new ApplicationException (); }).ConfigureAwait (false);
	}

	async Task TestException_3 ()
	{
		Func<Task> a = async () => await Task.Factory.StartNew (() => { throw new ApplicationException (); }).ConfigureAwait (false);
		await a ().ConfigureAwait (false);
	}
	
	async Task<int> TestException_4 ()
	{
		try {
			await Task.Factory.StartNew (() => 5).ConfigureAwait (false);
		} finally {
			throw new ApplicationException ();
		}
	}
	
	async Task<int> TestException_5 ()
	{
		int state = 0;
		try {
			await Task.Factory.StartNew (() => { throw new ArgumentException (); }).ConfigureAwait (false);
			state = 1;
		} catch (ArgumentException) {
			state = 2;
		} finally {
			if (state == 2)
				throw new ApplicationException ();	
		}
		
		return 1;
	}
	
	async Task<int> TestException_6 ()
	{
		try {
			await Task.Factory.StartNew (() => { throw new ArgumentException (); }).ConfigureAwait (false);
		} catch (ArgumentException) {
			throw new ApplicationException ();	
		}
		
		return 1;
	}

	async Task<int> TestException_7 ()
	{
		try {
			await Task.Factory.StartNew (() => { throw new ArgumentException (); }).ConfigureAwait (false);
		} catch (ArgumentException e) {
			if (e.StackTrace.Contains (".MoveNext") || e.StackTrace.Contains ("TestException_7 ()"))
				throw new ApplicationException ();	
		}
		
		return 1;
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
