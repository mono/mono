using System;

enum E : uint
{
	Value = 24
}

class A
{
	public static implicit operator sbyte (A mask)
	{
		return 1;
	}

	public static implicit operator byte (A mask)
	{
		return 2;
	}

	public static implicit operator short (A mask)
	{
		return 3;
	}

	public static implicit operator ushort (A mask)
	{
		return 4;
	}

	public static implicit operator int (A mask)
	{
		return 5;
	}

	public static implicit operator uint (A mask)
	{
		return 6;
	}

	public static implicit operator long (A mask)
	{
		return 7;
	}

	public static implicit operator ulong (A mask)
	{
		return 8;
	}
}

class A2
{
	public static implicit operator sbyte (A2 mask)
	{
		return 1;
	}

	public static implicit operator byte (A2 mask)
	{
		return 2;
	}

	public static implicit operator short (A2 mask)
	{
		return 3;
	}

	public static implicit operator uint (A2 mask)
	{
		return 6;
	}

	public static implicit operator long (A2 mask)
	{
		return 7;
	}

	public static implicit operator ulong (A2 mask)
	{
		return 8;
	}
}

class A3
{
	public static implicit operator sbyte (A3 mask)
	{
		return 1;
	}

	public static implicit operator uint (A3 mask)
	{
		return 6;
	}

	public static implicit operator long (A3 mask)
	{
		return 7;
	}

	public static implicit operator ulong (A3 mask)
	{
		return 8;
	}
}

class A4
{
	public static implicit operator uint (A4 mask)
	{
		return 6;
	}

	public static implicit operator long (A4 mask)
	{
		return 7;
	}

	public static implicit operator ulong (A4 mask)
	{
		return 8;
	}
}

class A5
{
	public static implicit operator uint (A5 mask)
	{
		return 6;
	}

	public static implicit operator int (A5 mask)
	{
		return 8;
	}
}

class A6
{
	public static implicit operator byte (A6 mask)
	{
		return 2;
	}
}

class MyDecimal
{
	public static implicit operator decimal (MyDecimal d)
	{
		return 42;
	}
}

public class Constraint
{
	public static int Main ()
	{
		A a = null;
		A2 a2 = null;
		A3 a3 = null;
		A4 a4 = null;
		A5 a5 = null;
		A6 a6 = null;

		if (-a != -5)
			return 1;
		if (-a2 != -3)
			return 2;
		if (-a3 != -1)
			return 3;
		if (-a4 != -7)
			return 4;
		if (-a5 != -8)
			return 5;
		if (-a6 != -2)
			return 6;

		if (~a != -6)
			return 10;
		if (~a2 != -4)
			return 11;
		if (~a3 != -2)
			return 12;
		if (~a4 != 4294967289)
			return 13;
		if (~a5 != -9)
			return 14;
		if (~a6 != -3)
			return 15;

		MyDecimal d = new MyDecimal ();
		if (-d != -42)
			return 20;

		E e = E.Value;
		if (~e != (E)4294967271)
			return 21;
			
		uint dp = 0;
		dp = +dp;			

		Console.WriteLine ("OK");
		return 0;
	}

}
