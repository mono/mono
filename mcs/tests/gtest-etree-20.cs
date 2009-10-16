using System;
using System.Linq.Expressions;

struct S
{
	public static int Main ()
	{
		Expression<Func<S?, A>> e = a => a;
		
		// TODO: implement
		// Console.WriteLine (e.Compile ()(null));
		
		Console.WriteLine (e.Compile ()(new S ()));
		
		Expression<Func<S?, B>> e2 = a => (B) a;
		
		// TODO: implement
		// Console.WriteLine (e2.Compile ()(null));
		
		Console.WriteLine (e2.Compile ()(new S ()));
		
		return 0;
	}
}

class A
{
	public static implicit operator A (S x)
	{
		return new B ();
	}
}

class B : A
{
}
