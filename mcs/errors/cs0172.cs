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
