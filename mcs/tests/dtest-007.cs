using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

// Dynamic member lookup tests


enum MyEnum : byte
{
	Value_1 = 1,
	Value_2 = 2
}

delegate void D ();

class Class
{
	internal int IntValue = 5;
	internal string StringStatic = "hi";

	public const decimal Decimal = -0.3m;
}

class Tester
{
	delegate void EmptyDelegate ();
	delegate int IntDelegate ();

	static void Assert<T> (T expected, T value, string name)
	{
		if (!EqualityComparer<T>.Default.Equals (expected, value)) {
			name += ": ";
			throw new ApplicationException (name + expected + " != " + value);
		}
	}

	static void AssertError (Action expected, string name)
	{
		try {
			expected ();
			throw new ApplicationException (name + ": RuntimeBinderException expected");
		} catch (RuntimeBinderException) {
			// passed
		}
	}

#pragma warning disable 169

	void MemberGetTest ()
	{
		dynamic d = new Class ();
		Assert (5, d.IntValue, "#1");

		Assert ("hi", d.StringStatic, "#2");

		// d.Event += delegate () { }; CS0019
	}

	void MemberGetError_Null ()
	{
		dynamic d = null;
		AssertError (() => { var v = d.Foo; }, "#1");
	}

	void MemberSetTest ()
	{
		dynamic d = new Class ();
		d.IntValue = 22;
		Assert (22, d.IntValue, "#1");

		d.StringStatic = "no";
		Assert ("no", d.StringStatic, "#2");
	}

	void MemberSetError_Null ()
	{
		dynamic d = null;
		AssertError (() => { d.Fo1 = 1; }, "#1");
	}

#pragma warning restore 169

	static bool RunTest (MethodInfo test)
	{
		Console.Write ("Running test {0, -25}", test.Name);
		try {
			test.Invoke (new Tester (), null);
			Console.WriteLine ("OK");
			return true;
		} catch (Exception e) {
			Console.WriteLine ("FAILED");
			Console.WriteLine (e.ToString ());
//			Console.WriteLine (e.InnerException.Message);
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

