using System;
using static TestClass;

internal class Program
{
	public static void Main (string[] args)
	{
		var res = Directions.Up;
	}
}

public enum Directions
{
	Up,
	NotUp,
}

public static class TestClass
{
	public static int Directions;
}
