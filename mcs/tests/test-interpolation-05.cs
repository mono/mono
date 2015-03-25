using System;

public class ParserTest
{
	void Test1 ()
	{
		bool b = true;
		Console.WriteLine(b ? $"{1:0.00}" : $"bar");
	}

	public static void Main()
	{
	}
}