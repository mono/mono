using System;

public class Tests
{
	public static int Main ()
	{
		return 0;
	}

	void Test1 ()
	{
		int a;
		if (true) {
			a = 0;
		} else {
			a = 1;
		}

		Console.WriteLine (a);
	}

	void Test2 ()
	{
		int a;
		if (false) {
			a = 0;
		} else {
			a = 1;
		}

		Console.WriteLine (a);
	}
}

