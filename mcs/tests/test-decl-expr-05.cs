class X
{
	void Test (string arg)
	{
		while (Call (out string s))
		{
			arg = s.ToString ();
		}

		while (true && Call (out string s2))
		{
			arg = s2.ToString ();
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