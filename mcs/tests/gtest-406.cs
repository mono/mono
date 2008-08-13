using System;
using System.Collections.Generic;

class Test<T>
{
	public void Foo <TOutput> (Func <T, TOutput> converter)
	{
	}
}

public class C<A, B>
{
	public C (IEnumerable<B> t)
	{
		new Test<B> ().Foo (a => a);
	}
}

class M
{
	public static void Main ()
	{
	}
}
