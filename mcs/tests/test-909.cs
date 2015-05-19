using System;

public struct S
{
	public int A { get; private set;}
	public event EventHandler eh;

	public S (int a)
	{
		this.eh = null;
		A = a;
	}

	public static void Main ()
	{
	}
}
