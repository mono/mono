using System;

public class Blah {

	static int Foo (string s)
	{
		return 2;
	}

	static int Foo (object o)
	{
		return 1;
	}

	public static int Main ()
	{
		int i = Foo (null);

		if (i == 1) {
			Console.WriteLine ("Wrong method ");
			return 1;
		}
		
		return 0;
	}

}



