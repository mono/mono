//
// This test excercises #pre-processor directives in non-1 column
// as well as the literal strings
//
class X {
	#if Blah
	#else
	static int Main ()
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

		string g = "Hello" + System.Environment.NewLine + "world";
		string h = @"Hello
world";
		if (g != h) 
			return 3;

		return 0;
	}
}
