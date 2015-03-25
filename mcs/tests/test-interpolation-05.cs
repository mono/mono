using System;

public class ParserTest
{
	void Test1 ()
	{
		bool b = true;
		Console.WriteLine(b ? $"{1:0.00}" : $"bar");
	}

	void Test2 ()
	{
		Console.WriteLine($"This should work but the compiler explodes if the string is too long!");		
	}

	public static void Main()
	{
	}
}