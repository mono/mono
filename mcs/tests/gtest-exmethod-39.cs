using System;
using Extra;

namespace Extra
{
	static class S
	{
		public static int Prefix (this string s, string prefix)
		{
			return 1;
		}
	}
}

static class SimpleTest
{
	public static int Prefix (this string s, string prefix, bool bold)
	{
		return 0;
	}
}

public class M
{
	public static int Main ()
	{
		var res = "foo".Prefix ("1");
		if (res != 1)
			return 1;
		
		return 0;
	}
}