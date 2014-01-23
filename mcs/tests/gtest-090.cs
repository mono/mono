using System;

public abstract class Foo<T>
{
	public virtual T InsertAll<U> (U u)
		where U : T
	{
		return u;
	}
}

public class Bar<T> : Foo<T>
{
	public override T InsertAll<U> (U u)
	{
		return u;
	}
}

class X
{
	public static void Main ()
	{ }
}
