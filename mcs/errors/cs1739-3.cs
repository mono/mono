// CS1739: The best overloaded method match for `System.Delegate.DynamicInvoke(params object[])' does not contain a parameter named `b'
// Line: 18

using System;

class C
{
	delegate int IntDelegate (int a);
	
	static int TestInt (int u)
	{
		return 29;
	}
	
	public static void Main ()
	{
		var del = new IntDelegate (TestInt);
		del.DynamicInvoke (b : 7);
	}
}
