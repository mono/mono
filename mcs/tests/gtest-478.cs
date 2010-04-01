using System;

interface IA
{
	void Foo ();
}

interface IG<T> : IA
{
	void GenFoo ();
}

class M : IG<int>
{
	public void Foo ()
	{
	}
	
	public void GenFoo ()
	{
	}
	
	public static void Main ()
	{
		IG<int> v = new M ();
		v.Foo ();
	}
}
