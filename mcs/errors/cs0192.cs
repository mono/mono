// cs0192.cs: You cannot pass a readonly field by ref or out (except in a constructor)
// Line: 17

using System;

class A
{
	public readonly int a=5;
	
	public void Inc (ref int a)
	{
		++a;
	}
	
	public void IncCall ()
	{
		Inc (ref a);
	}
	
	static void Main ()
	{
		Console.WriteLine ("Test cs0192.cs");
	}
}
