using System;

partial class A
{
}

partial class A
{
	public static int F = 3;
}

partial class B
{
	public static int F = 4;	
}

partial class B
{
}


public class C
{
	public static int Main ()
	{
		if (A.F != 3)
			return 1;
		
		if (B.F != 4)
			return 2;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
