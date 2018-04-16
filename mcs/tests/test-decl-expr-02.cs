using System;

public class C
{
	public static void Main ()
	{
	}

	bool Test1 => int.TryParse ("1", out int x);
	int Test2 => int.TryParse ("2", out int x) ? x : 0;
}