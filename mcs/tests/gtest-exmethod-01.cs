

using System;

static class SimpleTest
{
	public static string Prefix (this string s, string prefix)
	{
		return prefix + s;
	}
}

public class M
{
	public static int Main ()
	{
		SimpleTest.Prefix ("foo", "1");
		string s = "foo".Prefix ("1");
		
		Type ex_attr = typeof (System.Runtime.CompilerServices.ExtensionAttribute);
		if (!typeof (SimpleTest).IsDefined (ex_attr, false))
			return 1;
		
		if (!typeof (SimpleTest).Assembly.IsDefined (ex_attr, false))
			return 2;

		if (!typeof (SimpleTest).GetMethod ("Prefix").IsDefined (ex_attr, false))
			return 3;

		if (s != "1foo")
			return 9;
		
		Console.WriteLine (s);
		return 0;
	}
}