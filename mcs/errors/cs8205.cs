// cs8205.cs: A method that contains a yield statement does have an incorrect return type
// Line: 
using System;
using System.Collections;
class X {
	int Iterator ()
	{
		yield 1;
	}

	static void Main ()
	{
	}
}	
