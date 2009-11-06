using System;

delegate int D (int i = 1);

class C
{
	static int Foo (int i = 9)
	{
		return i;
	}
	
	public static int Main ()
	{
		D d = new D (Foo);
		var res = d ();
		if (res != 1)
			return 1;
		
		Console.WriteLine (res);
		return 0;
	}
}