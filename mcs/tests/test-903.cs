using System;

struct S
{
}

class C
{
	public static implicit operator S (C c)
	{
		return new S ();
	}
}

class Program
{
	static void Main ()
	{
		C c = new C ();
		var x = c ?? new S ();
	}
}