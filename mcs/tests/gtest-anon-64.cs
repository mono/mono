using System;

class C<T> where T : C<T>
{
	public static void Foo<U> (U arg) where U : C<U>, new ()
	{
		var i = new U ();
		{
			var ii = new U ();
			Func<U> f = () => i;
			{
				Action a = () => C<U>.Run (ii);
				a ();
			}
			f ();
		}
	}
	
	static void Run (T a)
	{
	}
}

class D : C<D>
{
}

class E : C<E>
{
}

class A
{
	public static int Main ()
	{
		D.Foo (new E ());
		return 0;
	}
}