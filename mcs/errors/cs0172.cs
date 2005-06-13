// cs0172.cs: Can not compute type of conditional expression as `X' and `Y' convert implicitly to each other
// Line: 25

class X {
	public static implicit operator X (Y y)
	{
		return null;
	}
}

class Y {
	public static implicit operator Y (X x)
	{
		return null;
	}
}

class Z
{
	static void Main ()
	{
		X x = new X ();
		Y y = new Y ();

		object d = (x == x) ? x : y;
	}
	
}
