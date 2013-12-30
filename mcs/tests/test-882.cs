using System;

public class MyUInt32
{
	public uint x;

	public MyUInt32 (uint x)
	{
		this.x = x;
	}

	public static implicit operator uint (MyUInt32 v)
	{
		return v.x;
	}

	public static implicit operator long (MyUInt32 v)
	{
		throw new ApplicationException ();
	}

	public static implicit operator MyUInt32 (uint v)
	{
		return new MyUInt32 (v);
	}

	public static implicit operator MyUInt32 (long v)
	{
		throw new ApplicationException ();
	}
}

class Test
{
	static MyUInt32 test1 (MyUInt32 x)
	{
		x = x + 1;
		return x;
	}

	static MyUInt32 test2 (MyUInt32 x)
	{
		x++;
		return x;
	}

	static MyUInt32 test3 (MyUInt32 x)
	{
		++x;
		return x;
	}

	public static int Main ()
	{
		var m = new MyUInt32 (2);
		m = test1 (m);
		if (m.x != 3)
			return 1;

		m = new MyUInt32 (2);
		m = test2 (m);
		if (m.x != 3)
			return 2;

		m = new MyUInt32 (3);
		m = test3 (m);
		if (m.x != 4)
			return 3;

		return 0;
	}
}