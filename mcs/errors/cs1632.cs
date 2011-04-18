// CS1632: Control cannot leave the body of an anonymous method
// Line: 12

using System;

class X {
	delegate void T ();

	static void Main ()
	{
		T t = delegate {
			goto L;
		};

L:
		Console.WriteLine ("Hello");
		      
	}
}
	
		
