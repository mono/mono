//
// Tests the various string implicit conversions
//

class XX {

	enum X {
		A = 1
	}
	
	public static int Main ()
	{
		int one = 1;
		int two = 2;
		
		if (("a" + "b") != "ab")
			return 1;

		if (("one" + one) != "one1")
			return 2;

		if ((one + "one") != "1one")
			return 3;

		if ((one + "two" + two) != "1two2")
			return 4;

		if ((X.A + "a") != "Aa")
			return 5;

		if (((int)X.A) + "a" != "1a")
			return 6;
	
		if ((1 + " " + "hello") != "1 hello")
			return 7;
	
		const string s1 = null + (string)null;
		const string s2 = (string)null + null;

		string s;
		s = (string)null + null;
		if (s.Length != 0)
			return 8;

		s = null + (string)null;
		if (s.Length != 0)
			return 9;

		s = (object)null + null;
		if (s.Length != 0)
			return 10;

		s = null + (object)null;
		if (s.Length != 0)
			return 11;

		s = (object)1 + null;
		if (s != "1")
			return 12;
	
		System.Console.WriteLine ("test ok");
		return 0;
	}
}
	
