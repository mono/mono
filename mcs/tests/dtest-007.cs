using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

// Dynamic member lookup tests

delegate void D ();

class Class
{
	internal int IntValue = 5;
	internal string StringStatic = "hi";

	public const decimal Decimal = -0.3m;

	uint s = 77;
	protected internal uint this[byte i] {
		get {
			return s * i;
		}
		set {
			s = value;
		}
	}

	byte b = 180;
	internal byte Prop {
		get {
			return b;
		}
		set {
			b = value;
		}
	}

	public int FixedValue
	{
		set { }
		get { return 823; }
	}
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

	void GetIndexTest ()
	{
		dynamic d = new[] { 5, 8, 2 };
		Assert (8, d[1], "#1");

		d = new int[,] { { 1, 2 }, { 3, 4 } };
		Assert (3, d[1, 0], "#2");

		dynamic d2 = new Class ();
		Assert<uint> (154, d2[2], "#3");
		Assert<uint> (154, d2[i:2], "#3a");
	}

	void GetIndexError_Null ()
	{
		dynamic d = null;
		AssertError (() => { var v = d[1]; }, "#1");
	}

	void MemberGetTest ()
	{
		dynamic d = new Class ();
		Assert (5, d.IntValue, "#1");
		Assert (180, d.Prop, "#1a");

		Assert ("hi", d.StringStatic, "#2");

		d = new int[4];
		Assert (4, d.Length, "#3");

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
		d.Prop = 19;
		Assert (19, d.Prop, "#1a");
		d.Prop++;
		Assert (20, d.Prop, "#1b");

		d.StringStatic = "no";
		Assert ("no", d.StringStatic, "#2");

		var r = d.FixedValue = 44;
		Assert (44, r, "#3");
	}

	void MemberSetError_Null ()
	{
		dynamic d = null;
		AssertError (() => { d.Fo1 = 1; }, "#1");
	}

	void SetIndexTest ()
	{
		dynamic d = new[] { "b", "v" };
		d[1] = "c";
		Assert ("c", d[1], "#1");

		d = new int[,] { { 1, 2 }, { 3, 4 } };
		d[1, 0] = 100;
		Assert (100, d[1, 0], "#2");
		d[1, 0]++;
		Assert (101, d[1, 0], "#2a");

		d [0, 0] = d [1, 0] = 55;
		Assert (55, d [0, 0], "#2a");

		dynamic d2 = new Class ();
		d2[2] = 500;
		Assert<uint> (1000, d2[2], "#3");
		d2[2]++;
		Assert<uint> (2002, d2[2], "#3a");
//		d2[i:1] = 3;
	//	Assert<uint> (3, d2[1], "#3b");
		
		uint r = d2 [1] = 200;
		Assert<uint> (200, r, "#4");
	}

	void SetIndexError_Null ()
	{
		dynamic d = null;
		AssertError (() => { d [1] = 0; }, "#1");
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

