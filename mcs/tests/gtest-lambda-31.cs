// Compiler options: -optimize

// Check lambdas to method group optimization, no lambdas should be created for any code int this test

using System;
using System.Collections;
using System.Linq;

class Test
{
	static int Prop {
		get {
			return 4;
		}
	}

	public static int Main ()
	{
		var parsed = from t in new[] { "2" } select int.Parse (t);
		if (parsed.First () != 2)
			return 1;

		var s = new string[] { "x", "a" };
		Array.Sort (s, (a, b) => String.Compare (a, b));
		if (s[0] != "a")
			return 10;

		if (s[1] != "x")
			return 11;

		Func<int> i = () => Prop;
		if (i () != 4)
			return 20;

		var em = new IEnumerable[] { new int[] { 1 } }.Select (l => l.Cast<int> ()).First ().First ();
		if (em != 1)
			return 30;

		Console.WriteLine ("ok");
		return 0;
	}
}