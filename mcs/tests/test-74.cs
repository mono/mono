
//
// This test excercises #pre-processor directives in non-1 column
// as well as the literal strings
// Warning: The first empty line is important

using System.IO;

class X {
	#if Blah
	#else
	public static int Main ()
	{
	#endif
		string s = @"Hola\";
		string d = "Hola\\";
		string e = @"Co""a";
		string f = "Co\"a";

		if (s != d)
			return 1;
		if (e != f)
			return 2;

		string g = "Hello\nworld";

		using (StreamReader sr = new StreamReader("test-74.cs")) {
			int i = sr.Read ();
			if (sr.Read () <= 13)
				g = g.Replace ("\n", "\r\n");
		}

		string h = @"Hello
world";
		if (g != h) 
			return 3;

		System.Console.WriteLine ("OK");
		return 0;
	}
}
