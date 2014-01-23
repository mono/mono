// Bug #62322.
using System;

public struct integer
{
	private readonly int value;

	public integer (int value)
	{
		this.value = value;
	}

	public static implicit operator integer (int i)
	{
		return new integer (i);
	}

	public static implicit operator double (integer i)
	{
		return Convert.ToDouble (i.value);
	}

	public static integer operator + (integer x, integer y)
	{
		return new integer (x.value + y.value);
	}
}

class X
{
	public static object Add (integer x, object other)
	{
		if (other is int) return x + ((int) other);
		if (other is double) return x + ((double) other);
		throw new InvalidOperationException ();
	}

	public static int Main ()
	{
		integer i = new integer (3);
		double d = 4.0;

		object result = Add (i, d);
		if (!(result is double))
			return 1;

		if ((double) result != 7.0)
			return 2;

		return 0;
	}
}
