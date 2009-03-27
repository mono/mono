using System;

class AA
{
	public virtual int Foo (int i)
	{
		return 1;
	}
}

abstract class A : AA
{
	public int Foo (byte b)
	{
		return 2;
	}
	
	public override int Foo (int i)
	{
		return 4;
	}
}

sealed class B : A
{
	public override int Foo (int i)
	{
		return 3;
	}
	
	public void Foo (string s)
	{
	}
}

struct S
{
	public override string ToString ()
	{
		return "aaaa";
	}
}

class MyClass
{
	public static int Main ()
	{
		S s = new S ();
		string sss = "a" + s.ToString ();
		
		B b = new B ();
		int res = b.Foo (1);
		
		if (res != 2)
			return res;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
