using System;

public struct Test
{
	public static Test op_Addition<T>(Test p1, T p2)
	{
		throw new ApplicationException ();
	}

	public static int op_Addition<T>(T p1, int p2)
	{
		throw new ApplicationException ();
	}

	public static Test operator +(Test p1, Test p2)
	{
		throw new ApplicationException ();
	}

	public static long operator +(Test p1, int p2)
	{
		return 4;
	}
}

public class Program
{
	public static int Main ()
	{
		var t = new Test ();

		int p2 = 20;
		var res = t + p2;
		if (res != 4)
			return 1;

		return 0;
	}
}