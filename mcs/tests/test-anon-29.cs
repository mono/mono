using System;
using System.Collections;

public class X
{
	string[] ABC = { "A", "B", "C" };
	string [,] EFGH = { { "E", "F" }, { "G", "H"}};

	delegate string Foo ();
	delegate void Bar (string s);

	public string Hello ()
	{
		Foo foo = delegate {
			foreach (string s in ABC){
				Bar bar = delegate (string t) {
					Console.WriteLine (t);
				};
				bar (s);
			}

			foreach (string s in EFGH){
				Bar bar = delegate (string t) {
					Console.WriteLine (t);
				};
				bar (s);
			}

			return "Hello";
		};
		return foo ();
	}

	public static void Main ()
	{
		X x = new X ();
		Console.WriteLine (x.Hello ());
	}
}
