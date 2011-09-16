// CS1913: Local variables of type `System.TypedReference' cannot be used inside anonymous methods, lambda expressions or query expressions
// Line: 9

using System;

class C
{
	public static void Main ()
	{
		int i = 1;
		TypedReference tr = __makeref (i);
		{
			Action a = () => {	TypedReference tr2 = tr; };
		}
	}
}

