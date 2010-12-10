// CS0120: An object reference is required to access non-static member `B.M()'
// Line: 16

using System;

class A
{
	protected A (Action a)
	{
	}
}

class B : A
{
	public B ()
		: base (M)
	{
	}
	
	void M ()
	{
	}
}
