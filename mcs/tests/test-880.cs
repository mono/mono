using System;

public class A
{
	public static void Main ()
	{
	}

	static void Test1 ()
	{
		int a;
		bool r = false;

		if (r && (a = 1) > 0 && r) {
			System.Console.WriteLine (a);
		}
	}

	static void Test2 ()
	{
		int a;
		var res = (a = 1) > 0 || Call (a);
	}

	static void Test3 ()
	{
		int a;
		if ((a = 1) > 0 || Call (a))
			return;
	}

	static void Test4 ()
	{
		int version1;
		bool r = false;
		if (r || !OutCall (out version1) || version1 == 0 || version1 == -1)
		{
			throw new ArgumentException();
		}
	}

	static void Test5 ()
	{
		bool r = false;
		int t1;
		if (Foo (r ? Call (1) : Call (4), OutCall (out t1)))
			Console.WriteLine (t1);
	}

	static void Test6 ()
	{
		int b = 0;
		var res = b != 0 && b.ToString () != null;
	}

	static bool Test7 ()
	{
		int f = 1;
		int g;
		return f > 1 && OutCall (out g) && g > 1;
	}

	static void Test8 ()
	{
		bool x = true;

		int a;
		if (x ? OutCall (out a) : OutCall (out a))
			System.Console.WriteLine (a);
		else
			System.Console.WriteLine (a);
	}

	static bool OutCall (out int arg)
	{
		arg = 1;
		return false;
	}
	
	static bool Call (int arg)
	{
		return false;
	}

	static bool Foo (params object[] arg)
	{
		return false;
	}
}