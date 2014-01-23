using System;

public class Foo<T>
{
	public void Map<S> (S value)
	{
		Foo<S> result = new Foo<S> ();
		result.Test (value);
	}

	protected virtual void Test (T value)
	{
		Console.WriteLine (value);
	}

}

class X
{
	public static void Main ()
	{
		Foo<double> a = new Foo<double> ();
		a.Map<string> ("Hello World");
	}
}
