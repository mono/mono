// cs8207.cs: out or ref are not allowed in an iterator method
//
using System;
using System.Collections;

class X {

	IEnumerator GetValue (int b, out int a)
	{
		yield 1;
	}
	
	static void Main ()
	{
	}
}
