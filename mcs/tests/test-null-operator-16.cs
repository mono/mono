using System;

class X
{
	Action<string> a;

	public static void Main ()
	{
		string x = null;
		string y = null;
		string[] z = null;

		x?.Contains (y?.ToLowerInvariant ());
		x?.Contains (y?.Length.ToString ());

		var res = x?[y?.Length ?? 0];

		var res2 = z?[x?.Length ?? 0];

		x?.Foo (y?.ToLowerInvariant ());

		X xx = null;
		xx?.a (y?.ToLowerInvariant ());
	}
}

static class E
{
	public static string Foo (this string arg, string value)
	{
		return "";
	}
}
