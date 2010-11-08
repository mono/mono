class A
{
	public int GetValues (string[] s, string value = null)
	{
		return 1;
	}

	public int GetValues (string s, params string [] args)
	{
		return 2;
	}
}


class B
{
	public static int Main ()
	{
		var a = new A ();
		if (a.GetValues (null) != 1)
			return 1;
		
		return 0;
	}
}