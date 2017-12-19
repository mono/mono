class X
{
	void Test (string arg)
	{
		while (Call (out string s))
		{
			arg = s.ToString ();
		}
	}

	static bool Call (out string s)
	{
		s = "";
		return true;
	}

	public static void Main ()
	{
		Call (out string s);
	}
}