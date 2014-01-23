using System;

class X
{
	public Artist Artist { get; set; }

	public static void Main ()
	{
		Test<string, Artist> (Artist.FromToken);
	}

	static void Test<T1, T2> (Func<T1, T2> arg)
	{
	}

	static void Test<T1, T2> (Func<T1, int, T2> arg)
	{
	}
}

public class Artist
{
	public static Artist FromToken (string token)
	{
		return null;
	}
}