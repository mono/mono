using System;

interface Blah {
	string this [ int INDEX ] { get; set; }
	string Item (int index);
}

public class Foo {

	public static void Main ()
	{
		Console.WriteLine ("foo");
	}
}
		
