// Compiler options: -langversion:latest

using System;

public static class B {
	public static void Main ()
	{
		int lo = 1;
		Bar (in lo);		
	}

	public static void Bar (in int arg)
	{
	}

	static void Foo (this in int src)
	{
		D p = (in int a) => {};
	}

}

delegate void D (in int arg);

class M
{
	int this[in int a] { set { } }
	public static implicit operator string (in M m) => null;
	public M (in int arg) { }

	public void Test2 (in int arg)
	{
	}
}