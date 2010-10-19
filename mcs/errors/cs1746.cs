// CS1746: The delegate `C.IntDelegate' does not contain a parameter named `b'
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
		del (b : 7);
	}
}
