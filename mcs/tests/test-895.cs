using System;

class X
{
	public void Test (int g, out int results)
	{
		if ((results = Foo (g > 0 ? 1 : 2)) != 4)
		{
			Console.WriteLine (results);
		}
	}

	int Foo (object o)
	{
		return 4;
	}

	public static void Main ()
	{
	}
}