using System;

class TestUnary
{
	static void Foo (Action<int> a)
	{
	}

	static void Bar ()
	{
		Foo (str => ++str);
	}
}

class Program
{

	static void Foo (Action<string> a)
	{
		a ("action");
	}

	static T Foo<T> (Func<string, T> f)
	{
		return f ("function");
	}
	
    static string Bar ()
	{
		return Foo (str => str.ToLower ());
	}

	public static void Main ()
	{
		var str = Foo (s => s);
		Console.WriteLine (str);
		Foo (s => Console.WriteLine (s));
	}
}
