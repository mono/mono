using System;

public class SimpleAttribute : Attribute {

	string n;
	
	public SimpleAttribute (string name)
	{
		n = name;
	}
}

public class Blah {

	public enum Foo {

		A,

		[Simple ("second")]
		B,

		C
	}

	public static int Main ()
	{
		//
		// We need a better test which does reflection to check if the
		// attributes have actually been applied etc.
		//

		return 0;
	}

}
