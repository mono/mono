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

		if (s != d)
			return 1;
		return 0;
	}
}
