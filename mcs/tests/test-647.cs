using System;

enum DT : byte {
	Foop
}

public class Foo
{
	public static void Main ()
	{
		DT dt;
		dt = (DT) byte.Parse ("123");
		dt = (DT) decimal.Parse ("123");
	}
}

