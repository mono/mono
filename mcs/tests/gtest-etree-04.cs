using System;
using System.Linq.Expressions;

struct Foo
{
	public static bool operator > (Foo d1, Foo d2)
	{
		throw new ApplicationException ();
	}
	
	public static bool operator < (Foo d1, Foo d2)
	{
		throw new ApplicationException ();
	}
	
	public static bool operator == (Foo d1, Foo d2)
	{
		throw new ApplicationException ();
	}
		
	public static bool operator != (Foo d1, Foo d2)
	{
		throw new ApplicationException ();
	}
}

class C
{
	public static int Main()
	{
		Foo f;
		Expression<Func<bool>> e = () => f > null;
		if (e.Compile ().Invoke ())
			return 1;
		
		e = () => f < null;
		if (e.Compile ().Invoke ())
			return 2;
		
		e = () => f == null;
		if (e.Compile ().Invoke ())
			return 3;
		
		e = () => f != null;
		if (!e.Compile ().Invoke ())
			return 4;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
