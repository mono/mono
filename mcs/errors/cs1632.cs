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
	
		
