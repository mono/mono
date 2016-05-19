using System;

public class A
{
	public A (Action action)
	{
	}
}

public class B : A
{
	public B () 
		: base (() => {
			dynamic d = 1;
			Test (d);
		})
	{
	}

	static decimal Test (dynamic arg)
	{
		return 3m;
	}
}

public class B2
{
	public Action a = () => {
			dynamic d = 1;
			Test (d);
		};

	static decimal Test (dynamic arg)
	{
		return 3m;
	}
}

class M
{
	static void Main ()
	{
		new B ();
		new B2 ();
	}	
}