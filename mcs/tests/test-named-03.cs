using System;

class C
{
	delegate int IntDelegate (int a);
	
	static int TestInt (int u)
	{
		return 29;
	}
	
	public static int Main ()
	{
		var del = new IntDelegate (TestInt);
		del (a : 7);
		
		return 0;
	}
}