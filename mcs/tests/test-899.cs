using System;

class Test
{
	public static void Main()
	{
		object o;
		lock (o = new object())
		{
			Console.WriteLine (o);
		}

		Console.WriteLine (o);
	}
}
