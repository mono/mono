using System;

static class MainClass
{
	public static void Main()
	{
		TestBug();
	}

	public static void TestBug()
	{
		int? value = null;
		value?.Test();
	}

	public static void Test(this int value)
	{
		Console.WriteLine("Not null");
	}
}
