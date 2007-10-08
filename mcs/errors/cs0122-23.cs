// CS0122: `C.this[int]' is inaccessible due to its protection level
// Line: 6

using System;
using System.Collections;

class C
{
	protected string this [int i] { set {} }
}

public class D
{
	void Foo ()
	{
		C c = new C ();
		c [0] = null;
	}
}
