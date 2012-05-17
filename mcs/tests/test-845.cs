interface IA
{
}

interface IB
{
	int Foo ();
	int Foo2 ();
}

interface IC : IA, IB
{
}

class C1 : IA, IB
{
	public int Foo ()
	{
		return 5;
	}
	
	public int Foo2 ()
	{
		return 55;
	}
}

class C2 : C1, IC
{
	public new int Foo ()
	{
		return 2;
	}
	
	public int Foo2 ()
	{
		return 2;
	}

	public static int Main ()
	{
		IC ic = new C2 ();
		if (ic.Foo () != 2)
			return 1;

		if (ic.Foo2 () != 2)
			return 2;
		
		return 0;
	}
}