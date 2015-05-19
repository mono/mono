using static T2;

static class T2
{
	public static int nameof (string s)
	{
		return 2;
	}
}

class X
{
	public static int Main ()
	{
		string s = "";
		var v = nameof (s);
		if (v != 2)
			return 1;

		return 0;
	}
}