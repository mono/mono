class X
{
	public readonly int value;

	public X (int value)
	{
		this.value = value;
	}

	public static implicit operator X (int y)
	{
		return new X (y);
	}
}

class Y
{
	public readonly X x;

	public Y (X x)
	{
		this.x = x;
	}

	public static implicit operator Y (X x)
	{
		return new Y (x);
	}
}

class Z
{
	public readonly Y y;

	public Z (Y y)
	{
		this.y = y;
	}

	public static implicit operator Z (Y y)
	{
		return new Z (y);
	}

	public static int Main ()
	{
		int a = 5;
		Y y = (Y) (X) a;

		//.
		// Compile this:
		//

		int b = (System.Int32)int.Parse ("1");
		return 0;
	}
}
