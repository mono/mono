using System;

class I {

	public delegate string GetTextFn (string a);

	static public GetTextFn GetText;

	static string fn (string s)
	{
		return "(" + s + ")";
	}
	
	static I ()
	{
		GetText = new GetTextFn (fn);
	}
}

class X {
	static void Main ()
	{
		I.GetTextFn _ = I.GetText;

		Console.WriteLine (_("Hello"));
	}
}
