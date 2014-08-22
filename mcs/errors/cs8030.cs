// CS8030: Anonymous function or lambda expression converted to a void returning delegate cannot return a value
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
