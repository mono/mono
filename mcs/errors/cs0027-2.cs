// CS0027: Keyword `this' is not available in the current context
// Line: 10

// Attention: Here the compiler complains saying that cannot convert implicitly from 'Error0027' to 'int' but
// should also say that the use of keyword 'this' is out of context since it's used outside a constructor, method
// or property.
using System;

class Error0027 {
	int i = this;
	int x = 0;
	
	public static void Main () {
		Console.WriteLine ("The compiler should complain: Error CS0027 trying to use 'this' outside context.");
		Console.WriteLine ("Trying to assign i to 'this' outside a method, property or ctr.");
	}
}

