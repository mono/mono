using System;

public struct S
{
	public int X { get; }
	public int Y { get; }

	public S ()
	{
		X = 4;
		Y = X;
	}

	public static int Main()
	{
		var s = new S ();
		if (s.Y != 4)
			return 1;

		return 0;
	}
}
