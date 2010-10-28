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
		
		return 0;
	}
}
