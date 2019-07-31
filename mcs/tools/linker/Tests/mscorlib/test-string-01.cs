using System;

public class C
{
	public static int Main ()
	{
		ReadOnlySpan<char> input = "Hello";
		var res = new string (input);

		return 0;
	}
}