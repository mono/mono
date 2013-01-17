// Compiler options: -r:test-anon-158-lib.dll

using System;

public class Test
{
	public X Foo<X> (bool b)
	{
		Call<X> foo = new Call<X> ();
		if (b) {
			Func<X> f = () => foo.Field;
			return f ();
		}
		
		throw null;
	}
	
	public X FooNested<X> (bool b)
	{
		Call<Call<X>> foo = new Call<Call<X>> ();
		foo.Field = new Call<X> ();
		if (b) {
			Func<Call<X>> f = () => foo.Field;
			return f ().Field;
		}
		
		throw null;
	}	
	
	public static int Main ()
	{
		var v = new Test ();
		if (v.Foo<int>(true) != 0)
			return 1;
			
		if (v.FooNested<int>(true) != 0)
			return 2;
		
		return 0;
	}
}
