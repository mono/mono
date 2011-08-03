using System;

class MainClass
{
	private static int TestParams (object ob = null, params object[] args)
	{
		if (ob != "a")
			return 1;

		if (args.Length != 4)
			return 2;

		foreach (object o in args) {
			Console.WriteLine (o);
		}

		return 0;
	}

	public static int Main ()
	{
		return TestParams ("a", "b", "c", "d", "e");
	}
}
