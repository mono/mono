// CS1988: Async methods cannot have ref or out parameters
// Line: 12

using System;

class C
{
	delegate void D (ref int i);
	
	public static void Main ()
	{
		D d = async delegate { };
	}
}
