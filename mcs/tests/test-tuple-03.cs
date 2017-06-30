using System;
using System.Linq.Expressions;

class TupleDeconstruct
{
	public static int Main ()
	{
//		var (xx, yy) = (1, 2);
//		if (xx != 1)
//			return 1;

//		if (yy != 2)
//			return 2;

		int x, y;
		(x, y) = (1, 2);
		if (x != 1)
			return 1;

		if (y != 2)
			return 2;

//		var (l1, l2) = ('a', 'b');

//		var cwd = new ClassWithDeconstruct ();
//		var (m1, m2) = cwd;

//		(string, string) ss = cwd; // Error

		return 0;
	}

	static void Test2 ()
	{
		var c = new C ();
		(c.Prop1, c.Prop2) = (1, 2);
	}

	static void var1 (object o1, object o2)
	{
	}

	static void TestCustom ()
	{
		return;
	}
}

class ClassWithDeconstruct
{
	public void Deconstruct (out string f, out string s)
	{
		f = "a";
		s = "z";
	}
}

class C
{
	public int Prop1 { get; set; }
	public int Prop2 { get; set; }
}