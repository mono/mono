using System;

class X {

	static string [] a = {
		"one", "two", "three"
	};
	
	static int Main ()
	{
		foreach (string s in a){
			Console.WriteLine (s);
		}

		return 0;
	}
}
