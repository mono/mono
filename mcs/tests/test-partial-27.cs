using System;

partial class C
{
	static partial void Partial (int i);
	
	static partial void Partial (string i);
	
	public static int Main ()
	{
		Partial (1);
		Partial ("x");
		return 0;
	}
}