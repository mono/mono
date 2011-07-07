// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.Linq;

struct S
{
	public int Value;
}

class Base
{
	protected int field_int;
	
	public bool PropertyBool {
		get {
			return true;
		}
	}
	
	static int Call (int arg1, int arg2, int arg3)
	{
		if (arg1 != 5)
			return 1;

		if (arg2 != -3)
			return 2;

		if (arg3 != 6)
			return 3;

		return 0;
	}
}

class Tester : Base
{
	async Task<int> AssignTest_1 ()
	{
		field_int = await Task.Factory.StartNew (() => 0);
		return field_int;
	}
	
	async Task<int> BinaryTest_1 ()
	{
		return await Task.Factory.StartNew (() => { Thread.Sleep (10); return 5; }) +
			await Task.Factory.StartNew (() => -3) +
			await Task.Factory.StartNew (() => -2);
	}
	
	async Task<int> BinaryTest_2 ()
	{
		int i = 1;
		var b = await Task.Factory.StartNew (() => { i += 3; return true; }) &&
			await Task.Factory.StartNew (() => { i += 4; return false; }) &&
			await Task.Factory.StartNew (() => { i += 5; return true; });

		return b ? -1 : i == 8 ? 0 : i;
	}
	
	async Task<int> CallTest_1 ()
	{
		/*
		return Call (
			await Task.Factory.StartNew (() => { Thread.Sleep (10); return 5; }),
			await Task.Factory.StartNew (() => -3),
			await Task.Factory.StartNew (() => 6));
		*/
		return 0;
	}
	
	async Task<int> ConditionalTest_1 ()
	{
		// TODO: problem with Resumable point setup when the expression never emitted
		//bool b = true;
		//return true ? await Task.Factory.StartNew (() => 0) : await Task.Factory.StartNew (() => 1);
		return 0;
	}
	
	async Task<int> ConditionalTest_2 ()
	{
		return PropertyBool ? await Task.Factory.StartNew (() => 0) : await Task.Factory.StartNew (() => 1);
	}
	
	async Task<int> ConditionalTest_3 ()
	{
		int v = 5;
		return v * (await Task.Factory.StartNew (() => true) ? 0 : await Task.Factory.StartNew (() => 1));
	}
	
	async Task<bool> NewArrayInitTest_1 ()
	{
		var a = new int[await Task.Factory.StartNew (() => 5)];
		return a.Length == 5;
	}
	
	async Task<bool> NewArrayInitTest_2 ()
	{
		var a = new short[await Task.Factory.StartNew (() => 3), await Task.Factory.StartNew (() => 4)];
		return a.Length == 12;
	}
	
	async Task<int> NewArrayInitTest_3 ()
	{
		var a = new byte[] { await Task.Factory.StartNew (() => (byte)5) };
		return a [0] - 5;
	}
	
	async Task<bool> NewArrayInitTest_4 ()
	{
		var a = new ushort[,] {
			{ await Task.Factory.StartNew (() => (ushort) 5), 50 },
			{ 30, await Task.Factory.StartNew (() => (ushort) 3) }
		};
		
		return a [0, 0] * a [1, 1] == 15;
	}
	
	async Task<int> NewArrayInitTest_5 ()
	{
		var a = new S[] { await Task.Factory.StartNew (() => new S () { Value = 4 }) };
		return a [0].Value - 4;
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
