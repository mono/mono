// CS0121: The call is ambiguous between the following methods or properties: `A.Foo(int, string)' and `A.Foo(string, int)'
// Line: 27

class A
{
	public virtual void Foo (int a2, string b2)
	{
	}
	
	public void Foo (string b, int a)
	{
	}
}

class B : A
{
	public override void Foo (int a, string b)
	{
	}
}

class C
{
	public static void Main ()
	{
		B b = new B ();
		b.Foo (a: 1, b: "x");
	}
}
