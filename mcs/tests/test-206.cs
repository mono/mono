using System;

interface I
{
	// int this [int i] { get; }
	int[] this [params int[] ii] { get; }
}

class X : I {
	public int this [int i] {
		get { return i; }
	}

	public int[] this [params int[] ii] {
		get { return new int[] { this[1], this[2], this[ii.Length] }; }
	}

	public static void Main ()
	{
		X x = new X ();
		Console.WriteLine (x [1]);
		int[] r = x [2, 2, 1, 2, 0];
		for (int i = 0; i < r.Length; i++)
			Console.Write (r [i] + " ");
		Console.WriteLine ();
	}
}
