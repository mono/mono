using System;

public interface IFoo<T>
{ }

public class Foo<T>
{
	public static bool Test (T x)
	{
		return x is IFoo<T>;
	}
}

class X
{
	static void Main ()
	{
		Foo<int>.Test (3);
	}
}
