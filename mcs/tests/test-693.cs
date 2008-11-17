using System;

public class ConstTest
{
	public static int Main ()
	{
		const float num = -2f;
		Console.WriteLine ("{0}, {1}", num, -num);
		return num != -num ? 0 : 1;
	}
}
