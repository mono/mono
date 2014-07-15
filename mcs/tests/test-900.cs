using System;

class D
{
	public static implicit operator D (Action d)
	{
		return new D ();
	}
}

class Program
{
	static void Main()
	{
		D d = (D) Main;
	}
}
