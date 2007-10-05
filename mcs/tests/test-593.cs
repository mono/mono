using System;

interface I
{
	void Finalize ();
}

class MainClass
{
	void Foo (I i)
	{
		i.Finalize ();
	}

	public static void Main ()
	{
	}
}
