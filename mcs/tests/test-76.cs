//
// This test is used to verify that we handle functions that have
// only an array parameter
//

using System;
using System.Text;

class foo {

	static string strcat (params string [] values)
	{
		StringBuilder s = new StringBuilder ();
		
		foreach (string val in values) {
			s.Append (val);
		}

		return s.ToString ();
	}

	public static int Main ()
	{
		if (strcat ("Hello", "World") != "HelloWorld")
			return 1;

		if (strcat () != "")
			return 2;

		if (strcat ("a", "b", "c", "d", "e") != "abcde")
			return 3;

		return 0;
	}
};
