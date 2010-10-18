using System;

public class A
{
	public virtual int Foo (int i)
	{
		return 1;
	}
	
	public virtual int this [int i, string s] {
		get {
			return 1;
		}
	}
}

public class B : A
{
	public new int Foo (int x)
	{
		return 2;
	}
	
	public new int this [int i, string s2] {
		get {
			return 2;
		}
	}
}

public class Test
{
	public static int Main ()
	{
		B p = new B ();
		if (p.Foo (i : 1) != 1)
			return 1;

		if (p.Foo (2) != 2)
			return 2;
		
		if (p [i : 1, s : "2"] != 1)
			return 3;
		
		if (p [i : 1, s2 : "2"] != 2)
			return 4;

		if (p [1, "2"] != 2)
			return 5;
		
		return 0;
	}
}

