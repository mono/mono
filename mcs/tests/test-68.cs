//
// Tests invocation of reference type functions with value type arguments
//
using System;
enum A {
	Hello
}

class X {

	static int Main ()
	{
		if ("Hello" != A.Hello.ToString ())
			return 1;

		Console.WriteLine ("value is: " + (5.ToString ()));
		if (5.ToString () != "5")
			return 2;
		
		return 0;
	}
}
	
