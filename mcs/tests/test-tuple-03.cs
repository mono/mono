using System;
using System.Linq.Expressions;

class TupleDeconstruct
{
	static int s_xx;
	static long s_yy;

	public static int Main ()
	{
		var (xx, yy) = (1, 2);
		if (xx != 1)
			return 1;

		if (yy != 2)
			return 2;

		int x, y;
		(x, y) = (1, 2);
		if (x != 1)
			return 3;

		if (y != 2)
			return 4;

		(s_xx, s_yy) = Test3 ();
		if (s_xx != 1)
			return 5;

		if (s_yy != 3)
			return 6;

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

	static (int, long) Test3 ()
	{
		return (1, 3);
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