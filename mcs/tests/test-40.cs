using System;

public class Blah {

	public enum MyEnum : byte {
		Foo = 254,
		Bar
	}
	
	public static int Main ()
	{
		Console.WriteLine ("Enum emission test");

		byte b = (byte) MyEnum.Foo;

		Console.WriteLine ("Foo has a value of " + b);

		if (b == 254)
			return 0;
		else
			return 1;
	}
}
