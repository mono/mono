using System;
public delegate void Simple ();

public delegate Simple Foo ();

class X
{
	public void Hello<U> (U u)
	{
	}

	public void Test<T> (T t)
	{
		{
			T u = t;
			Hello (u);
			Foo foo = delegate {
				T v = u;
				Hello (u);
				return delegate {
					Hello (u);
					Hello (v);
				};
			};
		}
	}

	static void Main ()
	{
	}
} 
