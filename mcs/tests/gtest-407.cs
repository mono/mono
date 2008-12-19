using System;

struct MyColor
{
	int v;

	public MyColor (int v)
	{
		this.v = v;
	}

	public static bool operator == (MyColor left, MyColor right)
	{
		return left.v == right.v;
	}

	public static bool operator != (MyColor left, MyColor right)
	{
		return left.v != right.v;
	}
}

public class NullableColorTests
{
	public static int Main ()
	{
		MyColor? col = null;
		bool b = col == new MyColor (3);
		Console.WriteLine (b);
		if (b)
			return 1;
			
		b = col != new MyColor (3);
		if (!b)
			return 2;
		
		return 0;
	}
}
