using System;

class A {
	public int foo = 1;
}

class B : A {
	public new int foo ()
	{
		return 1;
	}
	
	static void Main ()
	{
		B b = new B ();
		Console.WriteLine (b.foo ());
	}
}
