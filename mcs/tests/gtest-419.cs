using System;

class Program
{
	public static int Main ()
	{
		int? i = ((int?) -4);
		int? i2 = (int?) +4;
		object o = (object) (int?) 42;

		return (bool) Test ("True") == true ? 0 : 1;
	}

	static object Test (string s)
	{
		return (!string.IsNullOrEmpty (s)) ? (bool?) bool.Parse (s) : null;
	}
}
