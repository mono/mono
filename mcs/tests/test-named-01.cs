using System;

class A
{
	public int Index;
	
	public A ()
		: this (x : 0)
	{
	}
	
	protected A (object x)
	{
	}
	
	public virtual int this [int i] {
		set {
			Index = value;
		}
	}
}

class B : A
{
	public B ()
		: base (x : "x")
	{
	}
	
	public override int this [int i] {
		set {
			base [i : i] = value + 4;
		}
	}
}

class XAttribute:Attribute
{
	public XAttribute (int h)
	{
	}
}

[X (h : 3)]
class M
{
	static void Foo (int a)
	{
	}
	
	public static int Main ()
	{
		Foo (a : -9);
		
		B b = new B ();
		b [8] = 5;
		if (b.Index != 9)
			return 1;
		
		Console.WriteLine ("ok");
		return 0;
	}
}