using System;

public class SimpleAttribute : Attribute {

	string n;
	
	public SimpleAttribute (string name)
	{
		n = name;
	}
}

public class Blah {

	int i;

	public int Value {

		[Simple ("Foo!")]
		get {
			return i;
		}

		[Simple ("Bar !")]
		set {
			i = value;
		}
	}

	[Simple ((string) null)]
	int Another ()
	{
		return 1;
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
