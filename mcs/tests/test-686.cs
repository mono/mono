using System;

class Test
{
	public static void Main ()
	{
		string s = "test me";
		foreach (char c in s)
			Console.WriteLine (c);

		Foo ();
	}

	static void Foo ()
	{
		string [,] s = new string [,] { { "a", "b" }, { "c", "d" } };
		foreach (string c in s)
			Console.WriteLine (c [0]);
	}
}
