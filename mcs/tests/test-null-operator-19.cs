using System;

class C
{
	public S Acceleration { get; set;}
}

struct S
{
	public double X;
}

class X
{
	public static int Main ()
	{
		var c = new C();

		var g = c?.Acceleration.X;
		Console.WriteLine (g.GetType ());
		if (g.GetType () != typeof(double))
			return 1;

		return 0;
	}
}