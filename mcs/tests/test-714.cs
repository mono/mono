using System;

class Hello : IFoo
{
	void IBar.Test ()
	{
	}
	
	static void Main ()
	{
	}
}

interface IBar
{
	void Test ();
}

interface IFoo : IBar
{
}
