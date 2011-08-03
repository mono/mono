using System;

partial class C
{
	static partial void Partial (int i = 8);
	
	static partial void Partial (int i)
	{
		if (i != 8)
			throw new ApplicationException ();
	}
	
	public static int Main ()
	{
		Partial ();
		return 0;
	}
}