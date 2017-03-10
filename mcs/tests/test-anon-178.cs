using System;

public abstract class BaseClass<T>
{
}

public class DerivedClass : BaseClass<int>
{
}

public abstract class CA
{
	[Obsolete]
	public virtual void Foo<T, U> (U args) where T : BaseClass<U>, new()
	{
	}
}

public class CB : CA
{
	public CB ()
	{
		int x = 4;
		Action<int> pp = r => base.Foo<DerivedClass, int> (x);
	}

	public static void Main ()
	{
		new CB ();
	}
}