using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

// Dynamic member lookup tests

delegate void D ();
delegate void D2 (decimal d);

class Class
{
	internal int IntValue = 5;
	internal string StringStatic = "hi";

	public const decimal Decimal = -0.3m;

	public Class ()
	{
	}

	public Class (sbyte extra)
	{
		IntValue = extra;
	}

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

	internal string Method (string value)
	{
		return value;
	}
	
	public int Method (int a, byte b)
	{
		return a * b;
	}

	public void MethodInOut (ref int refValue, out string outValue)
	{
		refValue = 3;
		outValue = "4";
	}

	public static void GenericVoid<T> (T i)
	{
	}

	public static int StaticMethod (params int[] i)
	{
		return i [0] * i.Length;
	}
	
	public static void ArglistMethod (__arglist)
	{
	}
}

class EventClass
{
	internal event Func<int> OutEvent;
	
	public int CallEvent ()
	{
		return OutEvent ();
	}
}

class Tester
{
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
	
	event Func<int> e;
	int field;


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

	void InvokeTest ()
	{
		Func<string, string> f = new Class().Method;
		dynamic d = f;
		Assert ("bar", d ("bar"), "#1");

		Action<bool> f2 = Class.GenericVoid;
		d = f2;
		d (true);
		
		Func<string, int> f3 = (s) => 3;
		d = f3;
		d ("go");
	}

	void InvokeMember ()
	{
		dynamic d = new Class ();
		Assert ("vv", d.Method ("vv"), "#1");
		
		byte b = 2;
		Assert (6, d.Method (b: b++, a: b), "#1a");

		var arg1 = 1;
		var arg2 = "a";
		d.MethodInOut (ref arg1, out arg2);

		d = 2;
		Assert (2, Class.StaticMethod (d), "#2");
		Class.StaticMethod (d);	
	}
	
	void InvokeMember_Error ()
	{
		AssertError (() => {
				dynamic d_arg = "a";
				Class.ArglistMethod (d_arg);
			}, "#1");
	}

	void InvokeConstructor ()
	{
		dynamic d = (sbyte) 8;
		var r = new Class (d);
		Assert (8, r.IntValue, "#1");

		D2 method = (decimal e) => { };
		d = method;
		var r2 = new D2 (d);
	}

	void IsEvent ()
	{
		dynamic d = this;
		d.e += new Func<int> (() => 3);
		Assert (3, d.e (), "#1");
		
		d.field += 5;
		Assert (5, d.field, "#2");
		
		d = new EventClass ();
		d.OutEvent += new Func<int> (() => 100);
		Assert (100, d.CallEvent (), "#3");
	}

	void MemberGetTest ()
	{
		dynamic d = new Class ();
		Assert (5, d.IntValue, "#1");
		Assert (180, d.Prop, "#1a");

		Assert ("hi", d.StringStatic, "#2");

		d = new int[4];
		Assert (4, d.Length, "#3");
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

		d.Prop = byte.MaxValue;
		Assert (byte.MaxValue, d.Prop++, "#1b");
		Assert (1, ++d.Prop, "#1c");
		d.Prop++;
		Assert (2, d.Prop, "#1d");
		
		d.Prop += 5;
		Assert (7, d.Prop, "#1e");

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
		d2[i:1] = 3;
		Assert<uint> (3, d2[1], "#3b");
		
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

