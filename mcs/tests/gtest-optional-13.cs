using System;

class A
{
	public virtual int Foo (int i)
	{
		return i;
	}
	
	public virtual int Foo2 (int i = 99)
	{
		return i;
	}

	public virtual int this[string s, int arg] {
		get {
			return arg;
		}
	}
}

class B : A
{
	public override int Foo (int i2 = 4)
	{
		return i2 + 1;
	}
	
	public new int Foo2 (int i)
	{
		return 77;
	}

	public override int this[string s, int arg2 = 9] {
		get {
			return arg2 + 1;
		}
	}
}

class C
{
	public static int Main ()
	{
		B b = new B ();
		int i = b.Foo ();
		if (i != 5)
			return 1;

		i = b.Foo (i2: 3);
		if (i != 4)
			return 2;

		i = b["a"];
		if (i != 10)
			return 3;

		i = b["a", arg2: 20];
		if (i != 21)
			return 4;
		
		i = b.Foo2 ();
		if (i != 99)
			return 5;

		i = b.Foo2 (i : 8);
		if (i != 77)
			return 6;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
