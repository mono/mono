// CS0127: `System.Action': A return keyword must not be followed by any expression when delegate returns void
// Line: 10

using System;

class C
{
	public void Test ()
	{
		Action a = () => { return Skip (); };
	}
	
	void Skip ()
	{
	}
}
