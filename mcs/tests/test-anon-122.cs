using System.Linq.Expressions;

delegate int D1 ();
delegate long D2 ();

class C
{
	static int Foo (D1 d)
	{
		return 1;
	}
	
	static int Foo (D2 d)
	{
		return 2;
	}

	static int FooE (Expression<D1> d)
	{
		return 1;
	}
	
	static int FooE (Expression<D2> d)
	{
		return 2;
	}
	
	public static int Main ()
	{
		if (Foo (delegate () { return 1; }) != 1)
			return 1;

		FooE (() => 1);
		
		return 0;
	}
}
