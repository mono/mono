using System;

public class Test
{
	public static void Main ()
	{
		goto end;
		int a;
		Console.WriteLine ("unreachable");
	end:
		Console.WriteLine ("end");
	}
}
