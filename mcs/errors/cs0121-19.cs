// CS0121: The call is ambiguous between the following methods or properties: `C.M(System.Func<byte,int>)' and `C.M(System.Action<int>)'
// Line: 18

using System;

class C
{
	static void M (Func<byte,int> arg)
	{
	}
	
	static void M (Action<int> arg)
	{
	}

	static void Main()
	{
		M(l => l.GetHashCode());
	}
}
