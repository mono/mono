using System;
using System.Runtime.CompilerServices;

public interface X
{
	[IndexerName ("Foo")]
	int this [int a] {
		get;
	}
}

public class Y : X
{
	int X.this [int a] {
		get {
			return 1;
		}
	}

	[IndexerName ("Bar")]
	public int this [int a] {
		get {
			return 2;
		}
	}

	[IndexerName ("Bar")]
	public long this [double a] {
		get {
			return 3;
		}
	}
}

public class Z : Y
{
	[IndexerName ("Whatever")]
	new public long this [double a] {
		get {
			return 4;
		}
	}

	public int InstanceTest ()
	{
		double index = 5;

		Console.WriteLine ("INSTANCE TEST");

		if (this [index] != 4)
			return 6;
		if (base [index] != 3)
			return 7;

		return 0;
	}

	public static int Test ()
	{
		Z z = new Z ();
		X x = (X) z;
		Y y = (Y) z;

		Console.WriteLine (z [1]);
		Console.WriteLine (y [2]);
		Console.WriteLine (x [3]);

		if (z [1] != 4)
			return 1;
		if (y [1] != 2)
			return 2;
		if (x [1] != 1)
			return 3;

		double index = 5;

		Console.WriteLine (z [index]);
		Console.WriteLine (y [index]);

		if (z [index] != 4)
			return 4;
		if (y [index] != 3)
			return 5;

		return z.InstanceTest ();
	}

	public static int Main ()
	{
		int result = Test ();

		Console.WriteLine ("RESULT: " + result);

		return result;
	}
}
