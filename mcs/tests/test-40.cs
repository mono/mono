using System;

public class Blah {

	public enum MyEnum : byte {
		Foo = 254,
		Bar
	}
	
	public static int Main ()
	{
		Console.WriteLine ("Enum emission test");

		return 0;
	}
}
