// CS1729: The type `object' does not contain a constructor that takes `1' arguments
// Line: 8

using System;

class C
{
	int a = "a";
	
	public C (string s)
		: base (1)
	{
	}
	
	public C (int i)
		: base (i)
	{
	}
}
