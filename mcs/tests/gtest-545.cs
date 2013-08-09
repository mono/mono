using System;

public static class ApplicationContext
{
	static bool Foo ()
	{
		return false;
	}
		
	public static int Main ()
	{
		bool? debugging = false;
		debugging = debugging | Foo ();
		
		bool res = debugging.Value;
		if (res)
			return 1;
		
		debugging = true;
		debugging = debugging & Foo ();
		if (res)
			return 2;
		
		int? re = 3 + (short?) 7;
		if (re != 10)
			return 3;
		
		int a = 2;
		int b = 2;
		int? c = (byte?)a + b;
		if (c != 4)
			return 4;
		
		c = a + (ushort?)b;
		if (c != 4)
			return 5;
		
		return 0;
	}
}
