// CS0034: Operator `==' is ambiguous on operands of type `A' and `A'
// Line: 36

using System;

struct A
{
	public static implicit operator string (A c)
	{
		return null;
	}

	public static implicit operator Delegate (A c)
	{
		return null;
	}
}


class Program
{
	public static void Main ()
	{
		bool b = new A () == new A ();
	}
}
