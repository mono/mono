// CS0027: Keyword `this' is not available in the current context
// Line: 7 

using System;

class Error0027 {
	int i = this.x;
	int x = 0;
	
	public static void Main () {
		Console.WriteLine ("The compiler should complain: Error CS0027 trying to use 'this' outside context.");
		Console.WriteLine ("Trying to assign i to 'this.x' outside a method, property or ctr.");
	}
}

