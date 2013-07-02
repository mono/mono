using System;

class Hello : IFoo
{
	void IBar.Test ()
	{
	}
	
	public static void Main ()
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
