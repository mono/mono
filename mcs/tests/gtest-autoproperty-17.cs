using System;

class MainClass
{
	abstract public class Bar
	{
		abstract public bool Condition { get; }
	}

	class Baz: Bar
	{
		public override bool Condition { get; } = true;
	}

	public static void Main (string[] args)
	{
	}
}
